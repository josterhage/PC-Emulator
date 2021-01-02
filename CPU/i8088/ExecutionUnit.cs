using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SystemBoard.SystemClock;


/*
 *  Copyright (C) 2020, James Osterhage
 *  
 *      This code heavily references:
 *      
 *      Intel. (1979). The 8086 family user's manual. Santa Clara, CA: Intel. (http://bitsavers.org/components/intel/8086/9800722-03_The_8086_Family_Users_Manual_Oct79.pdf)
 *      IBM. (1981). 5150 Technical Reference. Boca Raton, FL: IBM Corporation. (http://www.minuszerodegrees.net/manuals/IBM_5150_Technical_Reference_6322507_APR84.pdf)
 *      
 *      This random x86 Instruction-Set reference - https://c9x.me/x86/
 *      This post about undocumented opcodes hosted by the OS/2 Museum - http://www.os2museum.com/wp/undocumented-8086-opcodes-part-i/#:~:text=And%20even%20when%208086%20opcodes,raise%20an%20invalid%20instruction%20exception).&text=On%20the%208086%2C%20all%20undocumented,typically%20not%20something%20very%20useful.
 *      The September 1990 edition of the 8086 datasheet (Order number 231455-005)
 *      https://www.reenigne.org/blog/8086-microcode-disassembled/
 *      
 *      Tedious examination of assembled simple code
 */

namespace SystemBoard.i8088
{
    class ExecutionUnit
    {
        //TODO: does the execution unit need timer access? its operation is asynchronous by nature: it gets data from the queue one byte at a time, works on it, and blocks when no new data is available
        private readonly MainTimer mainTimer = MainTimer.GetInstance();
        //necessary
        private readonly Processor parent;


        public GeneralRegisters Registers { get; private set; } = new GeneralRegisters();
        public readonly FlagRegister flags = new FlagRegister();
        private byte Opcode
        {
            get => Opcode;
            set
            {
                Opcode = value;
                EUChangedEvent?.Invoke(this, new EUChangeEventArgs(Registers.Clone(), flags.Clone(), Opcode));
            }
        }

        public event EventHandler<EUChangeEventArgs> EUChangedEvent;

        private void on_register_change(object sender, EventArgs e)
        {
            EUChangedEvent?.Invoke(this, new EUChangeEventArgs(Registers.Clone(), flags.Clone(), Opcode));
        }

        private readonly Thread executionUnitThread;

        private Segment overrideSegment = Segment.DS;

        private bool isRunning = true;

        private bool repInstruction = false;
        private bool halted = false;

        public ExecutionUnit(Processor parent)
        {
            Registers.RegisterChangeHandler += on_register_change;
            flags.FlagsChangeHandler += on_register_change;
            this.parent = parent;
            InitializeInstructionSet();
            executionUnitThread = new Thread(new ThreadStart(run));
        }

        public void Run()
        {
            executionUnitThread.Start();
        }

        public void End()
        {
            isRunning = false;
        }

        /// <summary>
        /// Fetches the next byte from the queue then executes it
        /// </summary>
        private void run()
        {
            while (isRunning)
            {
                //check for pending interrupt
                Opcode = parent.GetNextFromQueue();
                instructions[Opcode]?.Invoke();
                zeroize_temps();
                overrideSegment = Segment.DS;
            }
        }

        // Temporary Registers

        private void zeroize_temps()
        {
            TempA = 0;
            TempB = 0;
            TempC = 0;
        }

        private byte tempAL = 0;
        private byte tempAH = 0;

        /// <summary>
        /// tempA is for data going *to* the bus
        /// </summary>
        private ushort TempA
        {
            get
            {
                return (ushort)((tempAH << 8) | tempAL);
            }
            set
            {
                tempAL = (byte)(value & 0x00FF);
                tempAH = (byte)((value & 0xFF00) >> 8);
            }
        }

        private byte tempBL = 0;
        private byte tempBH;

        /// <summary>
        /// tempB is for data coming *from* the bus
        /// tempBL is for segment override (extracted from the instruction code)
        /// </summary>
        private ushort TempB
        {
            get
            {
                return (ushort)((tempBH << 8) | tempBL);
            }
            set
            {
                tempBL = (byte)(value & 0x00FF);
                tempBH = (byte)((value & 0xFF00) >> 8);
            }
        }

        private byte tempCL;
        private byte tempCH;

        /// <summary>
        /// tempC is for pointers
        /// </summary>
        private ushort TempC
        {
            get
            {
                return (ushort)((tempCH << 8) | tempCL);
            }
            set
            {
                tempCL = (byte)(value & 0x00FF);
                tempCH = (byte)((value & 0xFF00) >> 8);
            }
        }

        // Enumerations

        private enum ModEncoding
        {
            registerDisplacement,
            byteDisplacement,
            wordDisplacement,
            registerRegister
        }

        private enum RmEncoding
        {
            BXSI, //0b000
            BXDI, //0b001
            BPSI, //0b010
            BPDI, //0b011
            SI,   //0b100
            DI,   //0b101
            BP,   //0b110
            BX    //0b111
        }

        //Instruction Set

        private Action[] instructions;

        public void InitializeInstructionSet()
        {
            instructions = new Action[]
            {
                        add_rm8_r8,     // 0x00 add reg8/mem8 reg8 
                        add_rm16_r16,   // 0x01 add reg16/mem16 reg16
                        add_r8_rm8,     // 0x02 add reg8 reg8/mem8
                        add_r16_rm16,   // 0x03 add reg16 reg16/mem16
                        add_al_i8,      // 0x04 add al imm8
                        add_ax_i16,     // 0x05 add ax imm16
                        push_es,        // 0x06 push es
                        pop_es,         // 0x07 pop es
                        or_rm8_r8,      // 0x08 or reg8/mem8 reg8
                        or_rm16_r16,    // 0x09 or reg16/mem16 reg16
                        or_r8_rm8,      // 0x0a or reg8 reg8/mem8
                        or_r16_rm16,    // 0x0b or reg16 reg16/mem16
                        or_al_i8,       // 0x0c or al imm8
                        or_ax_i16,      // 0x0d or ax imm16
                        push_cs,        // 0x0e push cs
#if FULLFEATURE
                        pop_cs,         // 0x0f pop cs
#else
                        null,           // 0x0f NC
#endif
                        adc_rm8_r8,     // 0x10 adc reg8/mem8 reg8
                        adc_rm16_r16,   // 0x11 adc reg16/mem16 reg16
                        adc_r8_rm8,     // 0x12 adc reg8 reg8/mem8
                        adc_r16_rm16,   // 0x13 adc reg16 reg16/mem16
                        adc_al_i8,      // 0x14 adc imm8
                        adc_ax_i16,     // 0x15 adc imm16
                    
                        push_ss,        // 0x16 push ss
                        pop_ss,         // 0x17 pop ss
                    
                        sbb_rm8_r8,     // 0x18 sbb reg8/mem8 reg8
                        sbb_rm16_r16,   // 0x19 sbb reg16/mem16 reg16
                        sbb_r8_rm8,     // 0x1a sbb reg8 reg8/mem8
                        sbb_r16_rm16,   // 0x1b sbb reg16 reg16/mem16
                        sbb_al_i8,      // 0x1c sbb imm8
                        sbb_ax_i16,     // 0x1d sbb imm16
                    
                        push_ds,        // 0x1e push ds
                        pop_ds,         // 0x1f pop ds
                    
                        and_rm8_r8,     // 0x20 and reg8/mem8 reg8
                        and_rm16_r16,   // 0x21 and reg16/mem16 reg16
                        and_r8_rm8,     // 0x22 and reg8 reg8/mem8
                        and_r16_rm16,   // 0x23 and reg16 reg16/mem16
                        and_al_i8,      // 0x24 and imm8
                        and_ax_i16,     // 0x25 and imm16
                    
                        override_es,    // 0x26 segment override - es
                        daa,            // 0x27 daa
                    
                        sub_rm8_r8,     // 0x28 sub reg8/mem8 reg8
                        sub_rm16_r16,   // 0x29 sub reg16/mem16 reg16
                        sub_r8_rm8,     // 0x2a sub reg8 reg8/mem8
                        sub_r16_rm16,   // 0x2b sub reg16 reg16/mem16
                        sub_al_i8,      // 0x2c sub imm8
                        sub_ax_i16,     // 0x2d sub imm16
                   
                        override_cs,    // 0x2e segment override - cs
                        das,            // 0x2f das
                   
                        xor_rm8_r8,     // 0x30 xor reg8/mem8 reg8
                        xor_rm16_r16,   // 0x31 xor reg16/mem16 reg16
                        xor_r8_rm8,     // 0x32 xor reg8 reg8/mem8
                        xor_r16_rm16,   // 0x33 xor reg16 reg16/mem16
                        xor_al_i8,      // 0x34 xor imm8
                        xor_ax_i16,     // 0x35 xor imm16
                   
                        override_ss,    // 0x36 segment override - ss
                        aaa,            // 0x37 aaa
                   
                        cmp_rm8_r8,     // 0x38 cmp reg8/mem8 reg8
                        cmp_rm16_r16,   // 0x39 cmp reg16/mem16 reg16
                        cmp_r8_rm8,     // 0x3a cmp reg8 reg8/mem8
                        cmp_r16_rm16,   // 0x3b cmp reg16 reg16/mem16
                        cmp_al_i8,      // 0x3c cmp imm8
                        cmp_ax_i16,     // 0x3d cmp imm16
                  
                        override_ds,    // 0x3e segment override - ds
                        aas,            // 0x3f aas
                  
                        inc_ax,         // 0x40 inc ax
                        inc_cx,         // 0x41 inc cx
                        inc_dx,         // 0x42 inc dx
                        inc_bx,         // 0x43 inc bx
                        inc_sp,         // 0x44 inc sp
                        inc_bp,         // 0x45 inc bp
                        inc_si,         // 0x46 inc si
                        inc_di,         // 0x47 inc di
                 
                        dec_ax,         // 0x48 dec ax
                        dec_cx,         // 0x49 dec cx
                        dec_dx,         // 0x4a dec dx
                        dec_bx,         // 0x4b dec bx
                        dec_sp,         // 0x4c dec sp
                        dec_bp,         // 0x4d dec bp
                        dec_si,         // 0x4e dec si
                        dec_di,         // 0x4f dec di
                 
                        push_ax,        // 0x50 push ax
                        push_cx,        // 0x51 push cx
                        push_dx,        // 0x52 push dx
                        push_bx,        // 0x53 push bx
                        push_sp,        // 0x54 push sp
                        push_bp,        // 0x55 push bp
                        push_si,        // 0x56 push si
                        push_di,        // 0x57 push di
                
                        pop_ax,         // 0x58 pop ax
                        pop_cx,         // 0x59 pop cx
                        pop_dx,         // 0x5a pop dx
                        pop_bx,         // 0x5b pop bx
                        pop_sp,         // 0x5c pop sp
                        pop_bp,         // 0x5d pop bp
                        pop_si,         // 0x5e pop si
                        pop_di,         // 0x5f pop di
               
                        //0x60-0x6f
                        null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,

                        //Short jumps (all to inc8)
                        jo,             // 0x70 jo
                        jno,            // 0x71 jno
                        jb_jnae_jc,     // 0x72 jb/jnae/jc
                        jnb_jae_jnc,    // 0x73 jnb/jae/jnc
                        je_jz,          // 0x74 je/jz
                        jne_jnz,        // 0x75 jne/jnz
                        jbe_jna,        // 0x76 jbe/jna
                        jnbe_ja,        // 0x77 jnbe/ja
                        js,             // 0x78 js
                        jns,            // 0x79 jns
                        jp_jpe,         // 0x7a jp/jpe
                        jnp_jpo,        // 0x7b jnp/jpo
                        jl_jnge,        // 0x7c jl/jnge
                        jnl_jge,        // 0x7d jnl/jge
                        jle_jng,        // 0x7e jle/jng
                        jnle_jg,        // 0x7f jnle/jg

                        //Intel's ridiculous multiplexed instructions. there are entire 16-instruction empty blocks
                        //which instruction is determined by bits 4-6 of byte 2:
                        //000 - add, 001 - or, 010 - adc, 011 - sbb, 100 - and, 101 - sub, 110 - xor, 111 - cmp
                        o80h_rm8_i8,    // 0x80 reg8/mem8, imm8
                        o81h_rm16_i16,  // 0x81 reg16/mem16, imm16
                        //000 - add, 001 - NC, 010 - adc, 011 - sbb, 100 - NC, 101 - sub, 110 - NC, 111 - cmp
                        o82h_rm8_i16,   // 0x82 reg8/mem8, imm8
                        o83h_rm16_i16,  // 0x83 reg16/mem16, imm16
                    
                        test_rm8_r8,    // 0x84 test reg8/mem8, reg8
                        test_rm16_r16,  // 0x85 test reg16/mem16, reg16
                        xchg_rm8_r8,    // 0x86 xchg reg8, reg8/mem8
                        xchg_rm16_r16,  // 0x87 xchg reg16, reg16/mem16
                        mov_rm8_r8,     // 0x88 mov reg8/mem8, reg8
                        mov_rm16_r16,   // 0x89 mov reg16/mem16, reg16
                        mov_r8_rm8,     // 0x8a mov reg8, reg8/mem8
                        mov_r16_rm16,   // 0x8b mov reg16, reg16/mem16
                        mov_rm16_seg,   // 0x8c mov reg16/mem16, seg
                        lea_r16_m16,    // 0x8d lea reg16, mem16
                        mov_seg_rm16,   // 0x8e mov seg, reg16/mem16
                        pop_rm16,       // 0x8f pop reg16/mem16
                    
                        nop,            // 0x90 NOP (xchg ax,ax)
                        xchg_ax_cx,     // 0x91 xchg ax,cx [false]
                        xchg_ax_dx,     // 0x92 xchg ax,dx [false]
                        xchg_ax_bx,     // 0x93 xchg ax,bx [false]
                        xchg_ax_sp,     // 0x94 xchg ax,sp [false]
                        xchg_ax_bp,     // 0x95 xchg ax,bp [false]
                        xchg_ax_si,     // 0x96 xchg ax,si [false]
                        xchg_ax_di,     // 0x97 xchg ax,di [false]

                        cbw,            // 0x98 cbw (sign-extend AL into AH)
                        cwd,            // 0x99 cwd (sign-extend AX into DX)

                        call_far,       // 0x9a call disp-lo, disp-hi, seg-lo, seg-hi (assumedly sets CS to seg-hi:seg-lo and IP to disp-hi:disp-lo)

                        wait,           // 0x9b wait (x87 stuff?)

                        //push/pop flags transfer all flag bits
                        pushf,          // 0x9c pushf (push flags)
                        popf,           // 0x9d popf (pop flags)

                        //sahf/lahf only transfer bits 7,6,4,2,0 (SF,ZF,AF,PF,CF)
                        sahf,           // 0x9e sahf (ah -> flags)
                        lahf,           // 0x9f lahf (flags -> ah)

                        mov_al_m8,      // 0xa0 mov al, mem8 [false]
                        mov_ax_m16,     // 0xa1 mov ax, mem16 [false]
                        mov_m8_al,      // 0xa2 mov mem8, al [false]
                        mov_m16_ax,     // 0xa3 mov mem16, ax [false]

                        //array operations
                        movsb,          // 0xa4 movsb (copy byte at ds:si to es:di, inc/dec si/di according to DF)
                        movsw,          // 0xa5 movsw (copy word at ds:si to es:di, inc/dec si/di according to DF)
                        cmpsb,          // 0xa6 cmpsb (compare byte at ds:si to es:di, inc/dec si/di according to DF)
                        cmpsw,          // 0xa7 cmpsw (compare word at ds:si to es:di, inc/dec si/di according to DF)

                        test_al_i8,     // 0xa8 test al, imm8 (al AND imm8, set SF, ZF, PF, discard)
                        test_ax_i16,    // 0xa9 test ax, imm16 (ax AND imm16, set SF, ZF, PF, discard)

                        stosb,          // 0xaa stosb (al -> es:di, inc/dec di according to DF)
                        stosw,          // 0xab stosw (ax -> es:di, inc/dec di according to DF)
                        lodsb,          // 0xac lodsb (ds:si -> al, inc/dec si according to DF)
                        lodsw,          // 0xad lodsw (ds:si -> ax, inc/dec si according to DF)
                        scasb,          // 0xae scasb (compare al <-> es:di)
                        scasw,          // 0xaf scasw (compare ax <-> es:di)

                        // mov reg,imm8
                        mov_al_i8,      // 0xb0 al
                        mov_cl_i8,      // 0xb1 cl
                        mov_dl_i8,      // 0xb2 dl
                        mov_bl_i8,      // 0xb3 bl
                        mov_ah_i8,      // 0xb4 ah
                        mov_ch_i8,      // 0xb5 ch
                        mov_dh_i8,      // 0xb6 dh
                        mov_bh_i8,      // 0xb7 bh

                        //mov reg,imm16
                        mov_ax_i16,     // 0xb8 ax
                        mov_cx_i16,     // 0xb9 cx
                        mov_dx_i16,     // 0xba dx
                        mov_bx_i16,     // 0xbb bx
                        mov_sp_i16,     // 0xbc sp
                        mov_bp_i16,     // 0xbd bp
                        mov_si_i16,     // 0xbe si
                        mov_di_i16,     // 0xbf di

                        //0xc0-0xc1
#if FULLFEATURE
                        ret_near_i16,
                        ret_near,
#else
                        null,null,
#endif

                        //near returns
                        ret_near_i16,   // 0xc2 ret imm16 (return and pop imm16 bytes from stack)
                        ret_near,       // 0xc3 ret (return to caller)

                        //load segment
                        lds_rm16,       // 0xc4 lds reg16/mem16 (load effective address into ds)
                        les_rm16,       // 0xc5 les reg16/mem16 (load effective address into es)

                        mov_m8_i8,      // 0xc6 mov mem8, imm8
                        mov_m16_i16,    // 0xc7 mov mem16, imm16

                        //0xc8-0xc9
#if FULLFEATURE
                        ret_far_i16,
                        ret_far,
#else
                        null,null,
#endif
                        //far returns
                        ret_far_i16,    // 0xca ret imm16 (far return and pop imm16 bytes)
                        ret_far,        // 0xcb ret (far return)

                        //interrupts
                        int_3,          // 0xcc int 3 (debugger trap)
                        int_i8,         // 0xcd int imm8
                        int_o,          // 0xce into (overflow trap)
                        iret,           // 0xcf iret

                        //rotate/shift (multiplexed with modrm bits 4-6)
                        //000 - rol, 001 - ror, 010 - rcl, 011 - rcr, 100 - sal/shl, 101 - shr, 110 NC, 111 - SAR
                        // 1-bit
                        rot_shift1_rm8, // 0xd0 reg8/mem8
                        rot_shift1_rm16,// 0xd1 reg16/mem16
                        //cl-bits
                        rot_shiftn_rm8, // 0xd2 reg8/mem8
                        rot_shiftn_rm16,// 0xd3 reg16/mem16

                        aam,            // 0xd4 aam (byte 2 = 0x0a)
                        aad,            // 0xd5 aad (byte 2 = 0x0a)

#if FULLFEATURE
                        salc,           // 0xd6
#else
                        null,           // 0xd6
#endif

                        xlat,           // 0xd7 xlat (al = [ds:bx])

                        //co-processor escape prefixes
                        esc,         // 0xd8
                        esc,         // 0xd9
                        esc,         // 0xda
                        esc,         // 0xdb
                        esc,         // 0xdc
                        esc,         // 0xdd
                        esc,         // 0xde
                        esc,         // 0xdf

                        //short loops
                        loopne_nz_i8,   // 0xe0 loopne/loopnz inc8
                        loope_z_i8,     // 0xe1 loope/loopz inc8
                        loop_i8,        // 0xe2 loop inc8

                        jcxz_i8,        // 0xe3 jcxz inc8

                        //in/out 8-bit port
                        inb_i8,         // 0xe4 in byte [imm8]
                        inw_i8,         // 0xe5 in word [imm8]
                        outb_i8,        // 0xe6 out byte [imm8]
                        outw_i8,        // 0xe7 out word [imm8]

                        //calls/jumps
                        call_near_i16,  // 0xe8 call inc-lo, inc-hi (near)
                        jmp_near_i16,   // 0xe9 jmp inc-lo, inc-hi (near)
                        jmp_far_i16_i16,// 0xea jmp ip-lo, ip-hi, cs-lo, cs-hi (far)
                        jmp_short_i8,   // 0xeb jmp inc8 (short)

                        //in/out 16-bit port
                        inb_dx,         // 0xec in byte [dx]
                        inw_dx,         // 0xed in word [dx]
                        outb_dx,        // 0xee out byte [dx]
                        outw_dx,        // 0xef out word [dx]

                        lck,            // 0xf0 lock prefix

#if FULLFEATURE
                        lck,  
#else	
                        null,           // 0xf1
#endif

                        //repeat prefixes
                        repne_nz,       // 0xf2 repne/repnz
                        rep_e_nz,       // 0xf3 rep/repe/repz

                        halt,           // 0xf4 hlt

                        cmc,            // 0xf5 cmc

                        //multiplexed with modrm bits 4-6:
                        //000 - test, 001 - NC, 010 - NOT, 011 - NEG, 100 - MUL, 101 - IMUL, 110 - DIV, 111 - IDIV
                        oxf6_rm8,       // 0xf6
                        oxf7_rm16,      // 0xf7

                        clc,            // 0xf8
                        stc,            // 0xf9
                        cli,            // 0xfa
                        sti,            // 0xfb
                        cld,            // 0xfc
                        std,            // 0xfd

                        //multiplexed with modrm bits 4-6:
                        //000 - inc, 001 - dec, 010/011/100/101/110/111 - NC
                        inc_dec_rm8,    // 0xfe inc/dec reg8/mem8
                        //000 - inc, 001 - dec, 010 - call (near indirect), 011 - call (far indirect), 100 - jmp (near indirect),
                        //101 - jmp (far indirect), 110 - push mem16, 111 - nc
                        oxff            // 0xff
            };
        }

        private void add_rm8_r8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var destReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_sum(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                parent.WriteByte(overrideSegment, TempC, set_flags_and_sum(dest, Registers[srcReg]));
            }
        }

        private void add_rm16_r16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                var destReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_sum(Registers[destReg], Registers[srcReg]);
            }
            else
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                TempA = set_flags_and_sum(dest, Registers[srcReg]);

                parent.WriteWord(overrideSegment, TempC, TempA);
            }
        }

        private void add_r8_rm8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_sum(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();
                byte src = parent.ReadByte(overrideSegment, TempC);

                Registers[destReg] = set_flags_and_sum(Registers[destReg], src);
            }
        }

        private void add_r16_rm16()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_sum(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();

                ushort src = parent.ReadWord(overrideSegment, TempC);

                Registers[destReg] = set_flags_and_sum(Registers[destReg], src);
            }
        }

        private void add_al_i8()
        {
            fetch_next_from_queue();
            Registers.AL = set_flags_and_sum(Registers.AL, tempBL);
        }

        private void add_ax_i16()
        {
            fetch_next_from_queue();
            Registers.AX = set_flags_and_sum(Registers.AX, TempB);
        }

        private void push_es()
        {
            //decrement stack pointer by two (x86 stacks grow downward)
            Registers.SP -= 2;
            //write value of ES to SS:SP
            parent.WriteSegmentToMemory(Segment.ES, Registers.SP, Segment.SS);
        }

        private void pop_es()
        {
            //read the value at SS:SP
            TempA = parent.ReadWord(Segment.SS, Registers.SP);
            //tell the BIU to set ES
            parent.SetSegment(Segment.ES, TempA);
            //increment the stack pointer by two
            Registers.SP += 2;
            zeroize_temps();
            Opcode = parent.GetNextFromQueue();
            instructions[Opcode]?.Invoke();
        }

        private void or_rm8_r8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var destReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_or(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                //load the address
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                parent.WriteByte(overrideSegment, TempC, set_flags_and_or(dest, Registers[srcReg]));
            }
        }

        private void or_rm16_r16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                var destReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_or(Registers[destReg], Registers[srcReg]);
            }
            else
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                TempA = set_flags_and_or(Registers[srcReg], dest);

                parent.WriteWord(overrideSegment, TempC, set_flags_and_or(dest, Registers[srcReg]));
            }
        }

        private void or_r8_rm8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_or(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();
                var src = parent.ReadByte(overrideSegment, TempC);

                Registers[destReg] = set_flags_and_or(Registers[destReg], src);
            }
        }

        private void or_r16_rm16()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_or(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();
                ushort src = (ushort)(parent.ReadByte(overrideSegment, TempC) << 8);
                src |= parent.ReadByte(overrideSegment, (ushort)(TempC + 1));

                Registers[destReg] = set_flags_and_or(Registers[destReg], src);
            }
        }

        private void or_al_i8()
        {
            fetch_next_from_queue();
            Registers.AL = set_flags_and_or(Registers.AL, tempBL);
        }

        private void or_ax_i16()
        {
            fetch_next_from_queue();
            Registers.AX = set_flags_and_or(Registers.AX, TempB);
        }

        private void push_cs()
        {
            //decrement stack pointer by two (x86 stacks grow downward)
            Registers.SP -= 2;
            //write value of CS to SS:SP
            parent.WriteSegmentToMemory(Segment.CS, Registers.SP, Segment.SS);
        }

#if FULLFEATURE
            private void pop_cs()
            {
                //read the value at SS:SP
                TempA = busInterfaceUnit.GetWord(Segment.SS, registers.SP);
                //tell the BIU to set ES
                busInterfaceUnit.SetSegment(Segment.ES, TempA);
                //increment the stack pointer by two
                registers.SP += 2;
            }
#endif

        private void adc_rm8_r8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var destReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_sum_carry(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                parent.WriteByte(overrideSegment, TempC, set_flags_and_sum_carry(dest, Registers[srcReg]));
            }
        }

        private void adc_rm16_r16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                var destReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_sum_carry(Registers[destReg], Registers[srcReg]);
            }
            else
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                TempA = set_flags_and_sum_carry(dest, Registers[srcReg]);

                parent.WriteWord(overrideSegment, TempC, TempA);
            }
        }

        private void adc_r8_rm8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_sum_carry(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();
                byte src = parent.ReadByte(overrideSegment, TempC);

                Registers[destReg] = set_flags_and_sum_carry(Registers[destReg], src);
            }
        }

        private void adc_r16_rm16()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_sum_carry(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();

                ushort src = parent.ReadWord(overrideSegment, TempC);

                Registers[destReg] = set_flags_and_sum_carry(Registers[destReg], src);
            }
        }

        private void adc_al_i8()
        {
            fetch_next_from_queue();
            Registers.AL = set_flags_and_sum_carry(Registers.AL, tempBL);
        }

        private void adc_ax_i16()
        {
            fetch_next_from_queue();
            Registers.AX = set_flags_and_sum_carry(Registers.AX, TempB);
        }

        private void push_ss()
        {
            //decrement stack pointer by two (x86 stacks grow downward)
            Registers.SP -= 2;
            //write value of ES to SS:SP
            parent.WriteSegmentToMemory(Segment.SS, Registers.SP, Segment.SS);
        }

        private void pop_ss()
        {
            //read the value at SS:SP
            TempA = parent.ReadWord(Segment.SS, Registers.SP);
            //tell the BIU to set ES
            parent.SetSegment(Segment.SS, TempA);
            //increment the stack pointer by two
            Registers.SP += 2;
            zeroize_temps();
            Opcode = parent.GetNextFromQueue();
            instructions[Opcode]?.Invoke();
        }

        private void sbb_rm8_r8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var destReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_diff_borrow(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                parent.WriteByte(overrideSegment, TempC, set_flags_and_diff_borrow(dest, Registers[srcReg]));
            }
        }

        private void sbb_rm16_r16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                var destReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_diff_borrow(Registers[destReg], Registers[srcReg]);
            }
            else
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                TempA = set_flags_and_diff_borrow(dest, Registers[srcReg]);

                parent.WriteWord(overrideSegment, TempC, TempA);
            }
        }

        private void sbb_r8_rm8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_diff_borrow(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();
                byte src = parent.ReadByte(overrideSegment, TempC);

                Registers[destReg] = set_flags_and_diff_borrow(Registers[destReg], src);
            }
        }

        private void sbb_r16_rm16()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_diff_borrow(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();

                ushort src = parent.ReadWord(overrideSegment, TempC);

                Registers[destReg] = set_flags_and_diff_borrow(Registers[destReg], src);
            }
        }

        private void sbb_al_i8()
        {
            fetch_next_from_queue();
            Registers.AL = set_flags_and_diff_borrow(Registers.AL, tempBL);
        }

        private void sbb_ax_i16()
        {
            fetch_next_from_queue();
            Registers.AX = set_flags_and_diff_borrow(Registers.AX, TempB);
        }

        private void push_ds()
        {
            Registers.SP -= 2;
            parent.WriteSegmentToMemory(Segment.DS, Registers.SP, Segment.SS);
        }

        private void pop_ds()
        {
            TempA = parent.ReadWord(Segment.SS, Registers.SP);
            parent.SetSegment(Segment.SS, TempA);
            Registers.SP += 2;
            zeroize_temps();
            Opcode = parent.GetNextFromQueue();
            instructions[Opcode]?.Invoke();
        }

        private void and_rm8_r8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var destReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_and(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                //load the address
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                parent.WriteByte(overrideSegment, TempC, set_flags_and_and(dest, Registers[srcReg]));
            }
        }

        private void and_rm16_r16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                var destReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_and(Registers[destReg], Registers[srcReg]);
            }
            else
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                TempA = set_flags_and_and(Registers[srcReg], dest);

                parent.WriteWord(overrideSegment, TempC, set_flags_and_and(dest, Registers[srcReg]));
            }
        }

        private void and_r8_rm8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_and(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();
                var src = parent.ReadByte(overrideSegment, TempC);

                Registers[destReg] = set_flags_and_and(Registers[destReg], src);
            }
        }

        private void and_r16_rm16()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_and(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();
                ushort src = (ushort)(parent.ReadByte(overrideSegment, TempC) << 8);
                src |= parent.ReadByte(overrideSegment, (ushort)(TempC + 1));

                Registers[destReg] = set_flags_and_and(Registers[destReg], src);
            }
        }

        private void and_al_i8()
        {
            fetch_next_from_queue();
            Registers.AL = set_flags_and_and(Registers.AL, tempBL);
        }

        private void and_ax_i16()
        {
            fetch_next_from_queue();
            Registers.AX = set_flags_and_and(Registers.AX, TempB);
        }

        private void override_es()
        {
            overrideSegment = Segment.ES;
            fetch_next_from_queue();
            instructions[tempBL]?.Invoke();
        }

        private void daa()
        {
            var carry = flags.CF;
            tempAL = Registers.AL;
            flags.CF = false;

            if (((Registers.AL & 0x0f) > 9) || flags.AF)
            {
                Registers.AL += 6;
                flags.CF = carry;
                flags.AF = true;
            }
            else
            {
                flags.AF = false;
            }
            if ((tempAL > 0x99) || carry)
            {
                Registers.AL += 0x60;
                flags.CF = true;
            }
            else
            {
                flags.CF = false;
            }
        }

        private void sub_rm8_r8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var destReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_diff(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                parent.WriteByte(overrideSegment, TempC, set_flags_and_diff(dest, Registers[srcReg]));
            }
        }

        private void sub_rm16_r16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                var destReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_diff(Registers[destReg], Registers[srcReg]);
            }
            else
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                TempA = set_flags_and_diff(dest, Registers[srcReg]);

                parent.WriteWord(overrideSegment, TempC, TempA);
            }
        }

        private void sub_r8_rm8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_diff(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();
                byte src = parent.ReadByte(overrideSegment, TempC);

                Registers[destReg] = set_flags_and_diff(Registers[destReg], src);
            }
        }

        private void sub_r16_rm16()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_diff(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();

                ushort src = parent.ReadWord(overrideSegment, TempC);

                Registers[destReg] = set_flags_and_diff(Registers[destReg], src);
            }
        }

        private void sub_al_i8()
        {
            fetch_next_from_queue();
            Registers.AL = set_flags_and_diff(Registers.AL, tempBL);
        }

        private void sub_ax_i16()
        {
            fetch_next_from_queue();
            Registers.AX = set_flags_and_diff(Registers.AX, TempB);
        }

        private void override_cs()
        {
            overrideSegment = Segment.CS;
            fetch_next_from_queue();
            instructions[tempBL]?.Invoke();
        }

        private void das()
        {
            var carry = flags.CF;
            tempAL = Registers.AL;
            flags.CF = false;

            if (((Registers.AL & 0x0f) > 9) || flags.AF)
            {
                Registers.AL -= 6;
                flags.CF = carry;
                flags.AF = true;
            }
            else
            {
                flags.AF = false;
            }

            if ((tempAL > 0x99) || carry)
            {
                Registers.AL -= 0x60;
                flags.CF = true;
            }
            else
            {
                flags.CF = false;
            }
        }

        private void xor_rm8_r8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var destReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_xor(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                //load the address
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                parent.WriteByte(overrideSegment, TempC, set_flags_and_xor(dest, Registers[srcReg]));
            }
        }

        private void xor_rm16_r16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                var destReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_xor(Registers[destReg], Registers[srcReg]);
            }
            else
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                TempA = set_flags_and_xor(Registers[srcReg], dest);

                parent.WriteWord(overrideSegment, TempC, set_flags_and_xor(dest, Registers[srcReg]));
            }
        }

        private void xor_r8_rm8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_xor(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();
                var src = parent.ReadByte(overrideSegment, TempC);

                Registers[destReg] = set_flags_and_xor(Registers[destReg], src);
            }
        }

        private void xor_r16_rm16()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = set_flags_and_xor(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();
                ushort src = (ushort)(parent.ReadByte(overrideSegment, TempC) << 8);
                src |= parent.ReadByte(overrideSegment, (ushort)(TempC + 1));

                Registers[destReg] = set_flags_and_xor(Registers[destReg], src);
            }
        }

        private void xor_al_i8()
        {
            fetch_next_from_queue();
            Registers.AL = set_flags_and_xor(Registers.AL, tempBL);
        }

        private void xor_ax_i16()
        {
            fetch_next_from_queue();
            Registers.AX = set_flags_and_xor(Registers.AX, TempB);
        }

        private void override_ss()
        {
            overrideSegment = Segment.SS;
            fetch_next_from_queue();
            instructions[tempBL]?.Invoke();
        }

        private void aaa()
        {
            if (((Registers.AL & 0x0f) > 9) || flags.AF)
            {
                Registers.AX += 0x106;
                flags.AF = true;
                flags.CF = true;
            }
            else
            {
                flags.AF = false;
                flags.CF = false;
            }
            Registers.AL &= 0x0f;
        }

        private void cmp_rm8_r8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var destReg = (ByteGeneral)(tempBL & 0x07);

                tempAL = set_flags_and_diff(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                //load the address
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                tempAL = set_flags_and_diff(dest, Registers[srcReg]);
            }
        }

        private void cmp_rm16_r16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                var destReg = (WordGeneral)(tempBL & 0x07);

                TempA = set_flags_and_diff(Registers[destReg], Registers[srcReg]);
            }
            else
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                TempA = set_flags_and_diff(dest, Registers[srcReg]);
            }
        }

        private void cmp_r8_rm8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (ByteGeneral)(tempBL & 0x07);

                tempAL = set_flags_and_diff(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();
                byte src = parent.ReadByte(overrideSegment, TempC);

                tempAL = set_flags_and_diff(Registers[destReg], src);
            }
        }

        private void cmp_r16_rm16()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var srcReg = (WordGeneral)(tempBL & 0x07);

                TempA = set_flags_and_diff(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                //load the address
                build_effective_address();

                ushort src = parent.ReadWord(overrideSegment, TempC);

                TempA = set_flags_and_diff(Registers[destReg], src);
            }
        }

        private void cmp_al_i8()
        {
            fetch_next_from_queue();
            tempAL = set_flags_and_diff(Registers.AL, tempBL);
        }

        private void cmp_ax_i16()
        {
            fetch_next_from_queue();
            TempA = set_flags_and_diff(Registers.AX, TempB);
        }

        private void override_ds()
        {
            throw new NotImplementedException();
        }

        private void aas()
        {
            throw new NotImplementedException();
        }

        private void inc_ax()
        {
            //AF, OF, PF, SF, ZF;
            Registers.AX++;
            flags.AF = Registers.AX == 16;
            flags.OF = Registers.AX == 0x8000;
            set_parity(Registers.AX);
            set_sign(Registers.AX);
            flags.ZF = Registers.AX == 0;
        }

        private void inc_cx()
        {
            Registers.CX++;
            flags.AF = Registers.CX == 16;
            flags.OF = Registers.CX == 0x8000;
            set_parity(Registers.CX);
            set_sign(Registers.CX);
            flags.ZF = Registers.CX == 0;
        }

        private void inc_dx()
        {
            Registers.DX++;
            flags.AF = Registers.DX == 16;
            flags.OF = Registers.DX == 0x8000;
            set_parity(Registers.DX);
            set_sign(Registers.DX);
            flags.ZF = Registers.DX == 0;
        }

        private void inc_bx()
        {
            Registers.BX++;
            flags.AF = Registers.BX == 16;
            flags.OF = Registers.BX == 0x8000;
            set_parity(Registers.BX);
            set_sign(Registers.BX);
            flags.ZF = Registers.BX == 0;
        }

        private void inc_sp()
        {
            Registers.SP++;
            flags.AF = Registers.SP == 16;
            flags.OF = Registers.SP == 0x8000;
            set_parity(Registers.SP);
            set_sign(Registers.SP);
            flags.ZF = Registers.SP == 0;
        }

        private void inc_bp()
        {
            Registers.BP++;
            flags.AF = Registers.BP == 16;
            flags.OF = Registers.BP == 0x8000;
            set_parity(Registers.BP);
            set_sign(Registers.BP);
            flags.ZF = Registers.BP == 0;
        }

        private void inc_si()
        {
            Registers.SI++;
            flags.AF = Registers.SI == 16;
            flags.OF = Registers.SI == 0x8000;
            set_parity(Registers.SI);
            set_sign(Registers.SI);
            flags.ZF = Registers.SI == 0;
        }

        private void inc_di()
        {
            Registers.DI++;
            flags.AF = Registers.DI == 16;
            flags.OF = Registers.DI == 0x8000;
            set_parity(Registers.DI);
            set_sign(Registers.DI);
            flags.ZF = Registers.DI == 0;
        }

        private void dec_ax()
        {
            Registers.AX--;
            flags.AF = Registers.AX == 16;
            flags.OF = Registers.AX == 0x8000;
            set_parity(Registers.AX);
            set_sign(Registers.AX);
            flags.ZF = Registers.AX == 0;
        }

        private void dec_cx()
        {
            Registers.CX--;
            flags.AF = Registers.CX == 16;
            flags.OF = Registers.CX == 0x8000;
            set_parity(Registers.CX);
            set_sign(Registers.CX);
            flags.ZF = Registers.CX == 0;
        }

        private void dec_dx()
        {
            Registers.DX--;
            flags.AF = Registers.DX == 16;
            flags.OF = Registers.DX == 0x8000;
            set_parity(Registers.DX);
            set_sign(Registers.DX);
            flags.ZF = Registers.DX == 0;
        }

        private void dec_bx()
        {
            Registers.BX--;
            flags.AF = Registers.BX == 16;
            flags.OF = Registers.BX == 0x8000;
            set_parity(Registers.BX);
            set_sign(Registers.BX);
            flags.ZF = Registers.BX == 0;
        }

        private void dec_sp()
        {
            Registers.SP--;
            flags.AF = Registers.SP == 16;
            flags.OF = Registers.SP == 0x8000;
            set_parity(Registers.SP);
            set_sign(Registers.SP);
            flags.ZF = Registers.SP == 0;
        }

        private void dec_bp()
        {
            Registers.BP--;
            flags.AF = Registers.BP == 16;
            flags.OF = Registers.BP == 0x8000;
            set_parity(Registers.BP);
            set_sign(Registers.BP);
            flags.ZF = Registers.BP == 0;
        }

        private void dec_si()
        {
            Registers.SI--;
            flags.AF = Registers.SI == 16;
            flags.OF = Registers.SI == 0x8000;
            set_parity(Registers.SI);
            set_sign(Registers.SI);
            flags.ZF = Registers.SI == 0;
        }

        private void dec_di()
        {
            Registers.DI--;
            flags.AF = Registers.DI == 16;
            flags.OF = Registers.DI == 0x8000;
            set_parity(Registers.DI);
            set_sign(Registers.DI);
            flags.ZF = Registers.DI == 0;
        }

        private void push_ax()
        {
            Registers.SP -= 2;
            parent.WriteWord(Segment.SS, Registers.SP, Registers.AX);
        }

        private void push_cx()
        {
            Registers.SP -= 2;
            parent.WriteWord(Segment.SS, Registers.SP, Registers.CX);
        }

        private void push_dx()
        {
            Registers.SP -= 2;
            parent.WriteWord(Segment.SS, Registers.SP, Registers.DX);
        }

        private void push_bx()
        {
            Registers.SP -= 2;
            parent.WriteWord(Segment.SS, Registers.SP, Registers.BX);
        }

        private void push_sp()
        {
            Registers.SP -= 2;
            parent.WriteWord(Segment.SS, Registers.SP, Registers.SP);
        }

        private void push_bp()
        {
            Registers.SP -= 2;
            parent.WriteWord(Segment.SS, Registers.SP, Registers.BP);
        }

        private void push_si()
        {
            Registers.SP -= 2;
            parent.WriteWord(Segment.SS, Registers.SP, Registers.SI);
        }

        private void push_di()
        {
            Registers.SP -= 2;
            parent.WriteWord(Segment.SS, Registers.SP, Registers.DI);
        }

        private void pop_ax()
        {
            Registers.AX = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
        }

        private void pop_cx()
        {
            Registers.CX = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
        }

        private void pop_dx()
        {
            Registers.DX = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
        }

        private void pop_bx()
        {
            Registers.BX = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
        }

        private void pop_sp()
        {
            Registers.SP = parent.ReadWord(Segment.SS, Registers.SP);
            //testing appears to show that the value popped into SP is not changed
            //registers.SP += 2; 
        }

        private void pop_bp()
        {
            Registers.BP = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
        }

        private void pop_si()
        {
            Registers.SI = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
        }

        private void pop_di()
        {
            Registers.DI = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
        }

        private void jo()
        {
            fetch_next_from_queue();
            if (flags.OF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jno()
        {
            fetch_next_from_queue();
            if (!flags.OF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jb_jnae_jc()
        {
            fetch_next_from_queue();
            if (flags.CF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jnb_jae_jnc()
        {
            fetch_next_from_queue();
            if (!flags.CF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void je_jz()
        {
            fetch_next_from_queue();
            if (flags.ZF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jne_jnz()
        {
            fetch_next_from_queue();
            if (!flags.ZF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jbe_jna()
        {
            fetch_next_from_queue();
            if (flags.CF || flags.ZF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jnbe_ja()
        {
            fetch_next_from_queue();
            if (!flags.CF || !flags.ZF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void js()
        {
            fetch_next_from_queue();
            if (flags.SF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jns()
        {
            fetch_next_from_queue();
            if (!flags.SF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jp_jpe()
        {

            fetch_next_from_queue();
            if (flags.PF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jnp_jpo()
        {
            fetch_next_from_queue();
            if (!flags.PF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jl_jnge()
        {

            fetch_next_from_queue();
            if (flags.SF ^ flags.OF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jnl_jge()
        {

            fetch_next_from_queue();
            if (!(flags.SF ^ flags.OF))
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jle_jng()
        {
            fetch_next_from_queue();
            if ((flags.SF ^ flags.OF) || flags.ZF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void jnle_jg()
        {
            fetch_next_from_queue();
            if (!(flags.SF ^ flags.OF) || !flags.ZF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void o80h_rm8_i8()
        {
            fetch_next_from_queue();
            switch ((tempBL & 0x38) >> 3)
            {
                case 0:
                    add_rm8_i8();
                    break;
                case 1:
                    or_rm8_i8();
                    break;
                case 2:
                    adc_rm8_i8();
                    break;
                case 3:
                    sbb_rm8_i8();
                    break;
                case 4:
                    and_rm8_i8();
                    break;
                case 5:
                    sub_rm8_i8();
                    break;
                case 6:
                    xor_rm8_i8();
                    break;
                case 7:
                    cmp_rm8_i8();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void add_rm8_i8()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)(tempBL & 0x07);

                fetch_next_from_queue();

                Registers[destReg] = set_flags_and_sum(Registers[destReg], tempBL);
            }
            else
            {
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                fetch_next_from_queue();

                parent.WriteByte(overrideSegment, TempC, set_flags_and_sum(dest, tempBL));
            }
        }

        private void or_rm8_i8()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)(tempBL & 0x07);

                fetch_next_from_queue();

                Registers[destReg] = set_flags_and_or(Registers[destReg], tempBL);
            }
            else
            {
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                fetch_next_from_queue();

                parent.WriteByte(overrideSegment, TempC, set_flags_and_or(dest, tempBL));
            }
        }

        private void adc_rm8_i8()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)(tempBL & 0x07);

                fetch_next_from_queue();

                Registers[destReg] = set_flags_and_sum_carry(Registers[destReg], tempBL);
            }
            else
            {
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                fetch_next_from_queue();

                parent.WriteByte(overrideSegment, TempC, set_flags_and_sum_carry(dest, tempBL));
            }
        }

        private void sbb_rm8_i8()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)(tempBL & 0x07);

                fetch_next_from_queue();

                Registers[destReg] = set_flags_and_diff_borrow(Registers[destReg], tempBL);
            }
            else
            {
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                fetch_next_from_queue();

                parent.WriteByte(overrideSegment, TempC, set_flags_and_diff_borrow(dest, tempBL));
            }
        }

        private void and_rm8_i8()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)(tempBL & 0x07);

                fetch_next_from_queue();

                Registers[destReg] = set_flags_and_and(Registers[destReg], tempBL);
            }
            else
            {
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                fetch_next_from_queue();

                parent.WriteByte(overrideSegment, TempC, set_flags_and_and(dest, tempBL));
            }
        }

        private void sub_rm8_i8()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)(tempBL & 0x07);

                fetch_next_from_queue();

                Registers[destReg] = set_flags_and_diff(Registers[destReg], tempBL);
            }
            else
            {
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                fetch_next_from_queue();

                parent.WriteByte(overrideSegment, TempC, set_flags_and_diff(dest, tempBL));
            }
        }

        private void xor_rm8_i8()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)(tempBL & 0x07);

                fetch_next_from_queue();

                Registers[destReg] = set_flags_and_xor(Registers[destReg], tempBL);
            }
            else
            {
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                fetch_next_from_queue();

                parent.WriteByte(overrideSegment, TempC, set_flags_and_xor(dest, tempBL));
            }
        }

        private void cmp_rm8_i8()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)(tempBL & 0x07);

                fetch_next_from_queue();

                tempAL = set_flags_and_diff(Registers[destReg], tempBL);
            }
            else
            {
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                fetch_next_from_queue();

                tempAL = set_flags_and_sum(dest, tempBL);
            }
        }

        private void o81h_rm16_i16()
        {
            fetch_next_from_queue();
            switch ((tempBL & 0x38) >> 3)
            {
                case 0:
                    add_rm16_i16();
                    break;
                case 1:
                    or_rm16_i16();
                    break;
                case 2:
                    adc_rm16_i16();
                    break;
                case 3:
                    sbb_rm16_i16();
                    break;
                case 4:
                    and_rm16_i16();
                    break;
                case 5:
                    sub_rm16_i16();
                    break;
                case 6:
                    xor_rm16_i16();
                    break;
                case 7:
                    cmp_rm16_i16();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void add_rm16_i16()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (WordGeneral)(tempBL & 0x07);

                fetch_next_from_queue();
                tempAL = tempBL;

                fetch_next_from_queue();

                tempAH = tempBL;

                Registers[destReg] = set_flags_and_sum(Registers[destReg], TempA);
            }
            else
            {
                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                fetch_next_from_queue();

                tempAL = tempBL;

                fetch_next_from_queue();

                tempAH = tempBL;

                parent.WriteWord(overrideSegment, TempC, set_flags_and_sum(dest, TempA));
            }
        }

        private void or_rm16_i16()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (WordGeneral)(tempBL & 0x07);

                fetch_next_from_queue();
                tempAL = tempBL;

                fetch_next_from_queue();

                TempA |= (ushort)(tempBL << 8);

                Registers[destReg] = set_flags_and_or(Registers[destReg], TempA);
            }
            else
            {
                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                fetch_next_from_queue();

                tempAL = tempBL;

                fetch_next_from_queue();

                tempAH = tempBL;

                parent.WriteWord(overrideSegment, TempC, set_flags_and_or(dest, TempA));
            }
        }

        private void adc_rm16_i16()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (WordGeneral)(tempBL & 0x07);

                fetch_next_from_queue();
                tempAL = tempBL;

                fetch_next_from_queue();

                TempA |= (ushort)(tempBL << 8);

                Registers[destReg] = set_flags_and_sum_carry(Registers[destReg], TempA);
            }
            else
            {
                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                fetch_next_from_queue();

                tempAL = tempBL;

                fetch_next_from_queue();

                tempAH = tempBL;

                parent.WriteWord(overrideSegment, TempC, set_flags_and_sum_carry(dest, TempA));
            }
        }

        private void sbb_rm16_i16()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (WordGeneral)(tempBL & 0x07);

                fetch_next_from_queue();
                tempAL = tempBL;

                fetch_next_from_queue();

                TempA |= (ushort)(tempBL << 8);

                Registers[destReg] = set_flags_and_diff_borrow(Registers[destReg], TempA);
            }
            else
            {
                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                fetch_next_from_queue();

                tempAL = tempBL;

                fetch_next_from_queue();

                tempAH = tempBL;

                parent.WriteWord(overrideSegment, TempC, set_flags_and_diff_borrow(dest, TempA));
            }
        }

        private void and_rm16_i16()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (WordGeneral)(tempBL & 0x07);

                fetch_next_from_queue();
                tempAL = tempBL;

                fetch_next_from_queue();

                TempA |= (ushort)(tempBL << 8);

                Registers[destReg] = set_flags_and_and(Registers[destReg], TempA);
            }
            else
            {
                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                fetch_next_from_queue();

                tempAL = tempBL;

                fetch_next_from_queue();

                tempAH = tempBL;

                parent.WriteWord(overrideSegment, TempC, set_flags_and_and(dest, TempA));
            }
        }

        private void sub_rm16_i16()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (WordGeneral)(tempBL & 0x07);

                fetch_next_from_queue();
                tempAL = tempBL;

                fetch_next_from_queue();

                TempA |= (ushort)(tempBL << 8);

                Registers[destReg] = set_flags_and_diff(Registers[destReg], TempA);
            }
            else
            {
                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                fetch_next_from_queue();

                tempAL = tempBL;

                fetch_next_from_queue();

                tempAH = tempBL;

                parent.WriteWord(overrideSegment, TempC, set_flags_and_diff(dest, TempA));
            }
        }

        private void xor_rm16_i16()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (WordGeneral)(tempBL & 0x07);

                fetch_next_from_queue();
                tempAL = tempBL;

                fetch_next_from_queue();

                TempA |= (ushort)(tempBL << 8);

                Registers[destReg] = set_flags_and_xor(Registers[destReg], TempA);
            }
            else
            {
                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                fetch_next_from_queue();

                tempAL = tempBL;

                fetch_next_from_queue();

                tempAH = tempBL;

                parent.WriteWord(overrideSegment, TempC, set_flags_and_xor(dest, TempA));
            }
        }

        private void cmp_rm16_i16()
        {
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (WordGeneral)(tempBL & 0x07);

                fetch_next_from_queue();
                tempAL = tempBL;

                fetch_next_from_queue();

                TempA |= (ushort)(tempBL << 8);

                TempC = set_flags_and_diff(Registers[destReg], TempA);
            }
            else
            {
                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                fetch_next_from_queue();

                tempAL = tempBL;

                fetch_next_from_queue();

                tempAH = tempBL;

                TempC = set_flags_and_diff(dest, TempA);
            }
        }

        private void o82h_rm8_i16()
        {
            fetch_next_from_queue();
            switch ((tempBL & 0x38) >> 3)
            {
                case 0:
                    add_rm8_i8();
                    break;
                case 1:
                    break;
                case 2:
                    adc_rm8_i8();
                    break;
                case 3:
                    sbb_rm8_i8();
                    break;
                case 4:
                    break;
                case 5:
                    sub_rm8_i8();
                    break;
                case 6:
                    break;
                case 7:
                    cmp_rm8_i8();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void o83h_rm16_i16()
        {
            fetch_next_from_queue();
            switch ((tempBL & 0x38) >> 3)
            {
                case 0:
                    add_rm16_i16();
                    break;
                case 1:
                    break;
                case 2:
                    adc_rm16_i16();
                    break;
                case 3:
                    sbb_rm16_i16();
                    break;
                case 4:
                    break;
                case 5:
                    sub_rm16_i16();
                    break;
                case 6:
                    break;
                case 7:
                    cmp_rm16_i16();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void test_rm8_r8()
        {
            fetch_next_from_queue();
            //If bits 6/7 are high then the destination is a register
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                // in register-> register operations bits 3-5 of the ModRM byte encode the src
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                // and bits 0-2 encode the dest
                var destReg = (ByteGeneral)(tempBL & 0x07);

                tempAL = set_flags_and_and(Registers[destReg], Registers[srcReg]);
            }
            else //the destination is a memory address
            {
                //load the address
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                tempAL = set_flags_and_and(dest, Registers[srcReg]);
            }
        }

        private void test_rm16_r16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                var destReg = (WordGeneral)(tempBL & 0x07);

                TempA = set_flags_and_and(Registers[destReg], Registers[srcReg]);
            }
            else
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                build_effective_address();

                ushort dest = parent.ReadWord(overrideSegment, TempC);

                TempA = set_flags_and_and(Registers[srcReg], dest);

                TempA = set_flags_and_and(dest, Registers[srcReg]);
            }
        }

        private void xchg_rm8_r8()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                var destReg = (ByteGeneral)(tempBL & 0x07);

                tempAL = Registers[srcReg];
                Registers[srcReg] = Registers[destReg];
                Registers[destReg] = tempAL;
            }
            else
            {
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                build_effective_address();

                byte dest = parent.ReadByte(overrideSegment, TempC);

                tempAL = Registers[srcReg];
                Registers[srcReg] = dest;
                parent.WriteByte(overrideSegment, TempC, tempAL);
            }
        }

        private void xchg_rm16_r16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                var destReg = (WordGeneral)(tempBL & 0x07);

                TempA = Registers[srcReg];
                Registers[srcReg] = Registers[destReg];
                Registers[destReg] = TempA;
            }
            else
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                build_effective_address();

                ushort dest = parent.ReadByte(overrideSegment, TempC);

                TempA = Registers[srcReg];
                Registers[srcReg] = dest;
                parent.WriteWord(overrideSegment, TempC, TempA);
            }
        }

        private void mov_rm8_r8()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                var destReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = Registers[srcReg];
            }
            else
            {
                var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                build_effective_address();

                parent.WriteByte(overrideSegment, TempC, Registers[srcReg]);
            }
        }

        private void mov_rm16_r16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                var destReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = Registers[srcReg];
            }
            else
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                build_effective_address();

                parent.WriteWord(overrideSegment, TempC, Registers[srcReg]);
            }
        }

        private void mov_r8_rm8()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                var srcReg = (ByteGeneral)(tempBL & 0x07);

                Registers[destReg] = Registers[srcReg];
            }
            else
            {
                var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                build_effective_address();

                Registers[destReg] = parent.ReadByte(overrideSegment, TempC);
            }
        }

        private void mov_r16_rm16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                var srcReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = Registers[srcReg];
            }
            else
            {
                var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                build_effective_address();

                parent.WriteWord(overrideSegment, TempC, Registers[srcReg]);
            }
        }

        private void mov_rm16_seg()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (Segment)((tempBL & 0x18) >> 3);
                var destReg = (WordGeneral)(tempBL & 0x07);

                Registers[destReg] = parent.GetSegment(srcReg);
            }
        }

        private void lea_r16_m16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                throw new InvalidOperationException();

            var destReg = (WordGeneral)(tempBL & 0x07);

            build_effective_address();

            Registers[destReg] = TempC;
        }

        private void mov_seg_rm16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var srcReg = (WordGeneral)(tempBL & 0x07);
                var destReg = (Segment)((tempBL & 0x38) >> 3);

                parent.SetSegment(destReg, Registers[srcReg]);
            }
            else
            {
                var destReg = (Segment)((tempBL & 0x38) >> 3);

                build_effective_address();

                parent.ReadWord(overrideSegment, TempC);

                parent.SetSegment(destReg, TempB);
            }

            zeroize_temps();
            Opcode = parent.GetNextFromQueue();
            instructions[Opcode]?.Invoke();
        }

        private void pop_rm16()
        {
            fetch_next_from_queue();

            if ((tempBL & 0xc0) != 0)
                throw new InvalidOperationException();

            build_effective_address();

            TempA = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
            parent.WriteWord(overrideSegment, TempC, TempA);
        }

        private void nop()
        {
            TempA = Registers.AX;
            Registers.AX = Registers.AX;
            Registers.AX = TempA;
        }

        private void xchg_ax_cx()
        {

            TempA = Registers.AX;
            Registers.AX = Registers.CX;
            Registers.CX = TempA;
        }

        private void xchg_ax_dx()
        {

            TempA = Registers.AX;
            Registers.AX = Registers.DX;
            Registers.DX = TempA;
        }

        private void xchg_ax_bx()
        {

            TempA = Registers.AX;
            Registers.AX = Registers.BX;
            Registers.BX = TempA;
        }

        private void xchg_ax_sp()
        {

            TempA = Registers.AX;
            Registers.AX = Registers.SP;
            Registers.SP = TempA;
        }

        private void xchg_ax_bp()
        {
            TempA = Registers.AX;
            Registers.AX = Registers.BP;
            Registers.BP = TempA;
        }

        private void xchg_ax_si()
        {

            TempA = Registers.AX;
            Registers.AX = Registers.SI;
            Registers.SI = TempA;
        }

        private void xchg_ax_di()
        {

            TempA = Registers.AX;
            Registers.AX = Registers.DI;
            Registers.DI = TempA;
        }

        private void cbw()
        {
            Registers.AH = (byte)((Registers.AL & 0x80) != 0 ? 0xff : 0);
        }

        private void cwd()
        {
            Registers.DX = (ushort)((Registers.AX & 0x8000) != 0 ? 0xffff : 0);
        }

        private void call_far()
        {
            //push cs
            push_cs();
            //push IP
            parent.WriteIPToStack(Registers.SP);
            Registers.SP -= 2;

            //get the offset
            fetch_next_from_queue();
            tempAL = tempBL;
            fetch_next_from_queue();
            tempAH = tempBL;

            //get the segment
            fetch_next_from_queue();
            tempCL = tempBL;
            fetch_next_from_queue();
            tempCH = tempBL;

            //jump -> RET will assume that the next two values on the stack are the origin IP and CS, pop them, then jmp back
            parent.JumpFar(TempC, TempA);
        }

        private void wait()
        {
            parent.Wait();
        }

        private void pushf()
        {
            Registers.SP -= 2;
            parent.WriteWord(Segment.SS, Registers.SP, flags.Flags);
        }

        private void popf()
        {
            flags.Flags = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
        }

        private void sahf()
        {
            Registers.AH = (byte)(flags.Flags & 0xff);
        }

        private void lahf()
        {
            TempA = flags.Flags;
            tempAL = Registers.AH;
            flags.Flags = TempA;
        }

        private void mov_al_m8()
        {
            fetch_next_from_queue();
            tempAL = tempBL;
            fetch_next_from_queue();
            tempAH = tempBL;

            Registers.AL = parent.ReadByte(overrideSegment, TempA);
        }

        private void mov_ax_m16()
        {
            fetch_next_from_queue();
            tempAL = tempBL;
            fetch_next_from_queue();
            tempAH = tempBL;

            Registers.AX = parent.ReadWord(overrideSegment, TempA);
        }

        private void mov_m8_al()
        {
            fetch_next_from_queue();
            tempAL = tempBL;
            fetch_next_from_queue();
            tempAH = tempBL;

            parent.WriteByte(overrideSegment, TempA, Registers.AL);
        }

        private void mov_m16_ax()
        {
            fetch_next_from_queue();
            tempAL = tempBL;
            fetch_next_from_queue();
            tempAH = tempBL;

            parent.WriteWord(overrideSegment, TempA, Registers.AX);
        }

        private void movsb()
        {
            tempAL = parent.ReadByte(overrideSegment, Registers.SI);
            Registers.SI = flags.DF ? (ushort)(Registers.SI - 1) : (ushort)(Registers.SI + 1);
            parent.WriteByte(Segment.ES, Registers.DI, tempAL);
            Registers.DI = flags.DF ? (ushort)(Registers.DI - 1) : (ushort)(Registers.DI + 1);
        }

        private void movsw()
        {
            TempA = parent.ReadWord(overrideSegment, Registers.SI);
            Registers.SI = flags.DF ? (ushort)(Registers.SI - 2) : (ushort)(Registers.SI + 2);
            parent.WriteWord(Segment.ES, Registers.DI, TempA);
            Registers.DI = flags.DF ? (ushort)(Registers.DI - 2) : (ushort)(Registers.DI + 2);
        }

        private void cmpsb()
        {
            tempAL = parent.ReadByte(overrideSegment, Registers.SI);
            Registers.SI = flags.DF ? (ushort)(Registers.SI - 1) : (ushort)(Registers.SI + 1);
            tempBL = parent.ReadByte(Segment.ES, Registers.DI);
            Registers.DI = flags.DF ? (ushort)(Registers.DI - 1) : (ushort)(Registers.DI + 1);
            tempCL = set_flags_and_diff(tempAL, tempBL);
        }

        private void cmpsw()
        {
            TempA = parent.ReadWord(overrideSegment, Registers.SI);
            Registers.SI = flags.DF ? (ushort)(Registers.SI - 2) : (ushort)(Registers.SI + 2);
            TempB = parent.ReadWord(Segment.ES, Registers.DI);
            Registers.DI = flags.DF ? (ushort)(Registers.DI - 2) : (ushort)(Registers.DI + 2);
            TempC = set_flags_and_diff(TempA, TempB);
        }

        private void test_al_i8()
        {
            fetch_next_from_queue();
            tempAL = set_flags_and_and(Registers.AL, tempBL);
        }

        private void test_ax_i16()
        {
            fetch_next_from_queue();
            tempAL = tempBL;
            fetch_next_from_queue();
            tempAH = tempBL;
            TempB = set_flags_and_and(Registers.AH, TempA);
        }

        private void stosb()
        {
            parent.WriteByte(Segment.ES, Registers.DI, Registers.AL);
            Registers.DI = flags.DF ? (ushort)(Registers.DI - 1) : (ushort)(Registers.DI + 1);
        }

        private void stosw()
        {
            parent.WriteWord(Segment.ES, Registers.DI, Registers.AX);
            Registers.DI = flags.DF ? (ushort)(Registers.DI - 2) : (ushort)(Registers.DI + 2);
        }

        private void lodsb()
        {
            Registers.AL = parent.ReadByte(overrideSegment, Registers.SI);
            Registers.SI = flags.DF ? (ushort)(Registers.SI - 1) : (ushort)(Registers.SI + 1);
        }

        private void lodsw()
        {
            Registers.AX = parent.ReadWord(overrideSegment, Registers.SI);
            Registers.SI = flags.DF ? (ushort)(Registers.SI - 2) : (ushort)(Registers.SI + 2);
        }

        private void scasb()
        {
            tempAL = parent.ReadByte(Segment.ES, Registers.DI);
            Registers.DI = flags.DF ? (ushort)(Registers.DI - 1) : (ushort)(Registers.DI + 1);
            tempBL = set_flags_and_diff(Registers.AL, tempAL);
        }

        private void scasw()
        {
            TempA = parent.ReadWord(Segment.ES, Registers.DI);
            Registers.DI = flags.DF ? (ushort)(Registers.DI - 2) : (ushort)(Registers.DI + 2);
            TempB = set_flags_and_diff(Registers.AX, TempA);
        }

        private void mov_al_i8()
        {
            fetch_next_from_queue();
            Registers.AL = tempBL;
        }

        private void mov_cl_i8()
        {
            fetch_next_from_queue();
            Registers.CL = tempBL;
        }

        private void mov_dl_i8()
        {
            fetch_next_from_queue();
            Registers.DL = tempBL;
        }

        private void mov_bl_i8()
        {
            fetch_next_from_queue();
            Registers.BL = tempBL;
        }

        private void mov_ah_i8()
        {
            fetch_next_from_queue();
            Registers.AH = tempBL;
        }

        private void mov_ch_i8()
        {
            fetch_next_from_queue();
            Registers.CH = tempBL;
        }

        private void mov_dh_i8()
        {
            fetch_next_from_queue();
            Registers.DH = tempBL;
        }

        private void mov_bh_i8()
        {
            fetch_next_from_queue();
            Registers.BH = tempBL;
        }

        private void mov_ax_i16()
        {
            fetch_next_from_queue();
            Registers.AL = tempBL;
            fetch_next_from_queue();
            Registers.AH = tempBL;
        }

        private void mov_cx_i16()
        {
            fetch_next_from_queue();
            Registers.CL = tempBL;
            fetch_next_from_queue();
            Registers.CH = tempBL;
        }

        private void mov_dx_i16()
        {
            fetch_next_from_queue();
            Registers.DL = tempBL;
            fetch_next_from_queue();
            Registers.DH = tempBL;
        }

        private void mov_bx_i16()
        {
            fetch_next_from_queue();
            Registers.BL = tempBL;
            fetch_next_from_queue();
            Registers.BH = tempBL;
        }

        private void mov_sp_i16()
        {
            fetch_next_from_queue();
            tempCL = tempBL;
            fetch_next_from_queue();
            tempCH = tempBL;
            Registers.SP = TempC;
        }

        private void mov_bp_i16()
        {
            fetch_next_from_queue();
            tempCL = tempBL;
            fetch_next_from_queue();
            tempCH = tempBL;
            Registers.SP = TempC;
        }

        private void mov_si_i16()
        {
            fetch_next_from_queue();
            tempCL = tempBL;
            fetch_next_from_queue();
            tempCH = tempBL;
            Registers.SP = TempC;
        }

        private void mov_di_i16()
        {
            fetch_next_from_queue();
            tempCL = tempBL;
            fetch_next_from_queue();
            tempCH = tempBL;
            Registers.SP = TempC;
        }

        //0xc0 and 0xc1 have no instructions

        private void ret_near_i16()
        {
            TempA = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
            fetch_next_from_queue();
            Registers.SP += tempBL;
            parent.JumpImmediate(TempA);
        }

        private void ret_near()
        {
            TempA = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
            parent.JumpImmediate(TempA);
        }

        private void lds_rm16()
        {
            fetch_next_from_queue();
            var destReg = (WordGeneral)(tempBL & 0x07);
            build_effective_address();
            TempA = parent.ReadWord(overrideSegment, TempC);
            TempB = parent.ReadWord(overrideSegment, (ushort)(TempC + 2));
            parent.SetSegment(Segment.DS, TempA);
            Registers[destReg] = TempB;
        }

        private void les_rm16()
        {
            fetch_next_from_queue();
            var destReg = (WordGeneral)(tempBL & 0x07);
            build_effective_address();
            TempA = parent.ReadWord(overrideSegment, TempC);
            TempB = parent.ReadWord(overrideSegment, (ushort)(TempC + 2));
            parent.SetSegment(Segment.ES, TempA);
            Registers[destReg] = TempB;
        }

        private void mov_m8_i8()
        {
            fetch_next_from_queue();
            build_effective_address();
            fetch_next_from_queue();
            parent.WriteByte(overrideSegment, TempC, tempBL);
        }

        private void mov_m16_i16()
        {
            fetch_next_from_queue();
            build_effective_address();
            fetch_next_from_queue();
            tempAL = tempBL;
            fetch_next_from_queue();
            tempAH = tempBL;
            parent.WriteWord(overrideSegment, TempC, TempA);
        }

        //0xc8 and 0xc9 have no instructions

        private void ret_far_i16()
        {
            TempA = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
            TempB = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
            fetch_next_from_queue();
            Registers.SP += tempBL;
            parent.JumpFar(TempB, TempA);
        }

        private void ret_far()
        {
            TempA = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
            TempB = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
            parent.JumpFar(TempB, TempA);
        }

        private void int_3()
        {
            pushf();
            flags.IF = false;
            flags.TF = false;
            flags.AF = false;
            push_cs();
            parent.WriteIPToStack(Registers.SP);
            Registers.SP -= 2;
            parent.JumpToInterruptVector(3);
        }

        private void int_i8()
        {
            pushf();
            flags.IF = false;
            flags.TF = false;
            flags.AF = false;
            fetch_next_from_queue();
            push_cs();
            parent.WriteIPToStack(Registers.SP);
            Registers.SP -= 2;
            parent.JumpToInterruptVector(tempBL);
        }

        private void int_o()
        {
            if (flags.OF)
            {
                pushf();
                flags.IF = false;
                flags.TF = false;
                flags.AF = false;
                push_cs();
                parent.WriteIPToStack(Registers.SP);
                Registers.SP -= 2;
                parent.JumpToInterruptVector(4);
            }
        }

        private void iret()
        {
            TempA = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
            TempB = parent.ReadWord(Segment.SS, Registers.SP);
            Registers.SP += 2;
            popf();
            parent.JumpFar(TempB, TempA);
            zeroize_temps();
            overrideSegment = Segment.DS;
            Opcode = parent.GetNextFromQueue();
            instructions[Opcode]?.Invoke();
        }

        private void rot_shift1_rm8()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)(tempBL & 0x07);

                tempAL = Registers[destReg];

                switch ((byte)((tempBL & 0x38) >> 3))
                {
                    case 0: //rol
                        Registers[destReg] = rol(tempAL);
                        break;
                    case 1: //ror
                        Registers[destReg] = ror(tempAL);
                        break;
                    case 2: //rcl
                        Registers[destReg] = rcl(tempAL);
                        break;
                    case 3: //rcr
                        Registers[destReg] = rcr(tempAL);
                        break;
                    case 4: //sal/shl
                        Registers[destReg] = sal(tempAL);
                        break;
                    case 5: //shr
                        Registers[destReg] = shr(tempAL);
                        break;
                    case 6: //NC
                        break;
                    case 7: //SAR
                        Registers[destReg] = sar(tempAL);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                build_effective_address();

                tempAL = parent.ReadByte(overrideSegment, TempC);

                switch ((byte)((tempBL & 0x38) >> 3))
                {
                    case 0: //rol
                        parent.WriteByte(overrideSegment, TempC, rol(tempAL));
                        break;
                    case 1: //ror
                        parent.WriteByte(overrideSegment, TempC, ror(tempAL));
                        break;
                    case 2: //rcl
                        parent.WriteByte(overrideSegment, TempC, rcl(tempAL));
                        break;
                    case 3: //rcr
                        parent.WriteByte(overrideSegment, TempC, rcr(tempAL));
                        break;
                    case 4: //sal/shl
                        parent.WriteByte(overrideSegment, TempC, sal(tempAL));
                        break;
                    case 5: //shr
                        parent.WriteByte(overrideSegment, TempC, shr(tempAL));
                        break;
                    case 6: //NC
                        break;
                    case 7: //SAR
                        parent.WriteByte(overrideSegment, TempC, sar(tempAL));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private void rot_shift1_rm16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (WordGeneral)(tempBL & 0x07);

                TempA = Registers[destReg];

                switch ((byte)((tempBL & 0x38) >> 3))
                {
                    case 0: //rol
                        Registers[destReg] = rol(TempA);
                        break;
                    case 1: //ror
                        Registers[destReg] = ror(TempA);
                        break;
                    case 2: //rcl
                        Registers[destReg] = rcl(TempA);
                        break;
                    case 3: //rcr
                        Registers[destReg] = rcr(TempA);
                        break;
                    case 4: //sal/shl
                        Registers[destReg] = sal(TempA);
                        break;
                    case 5: //shr
                        Registers[destReg] = shr(TempA);
                        break;
                    case 6: //NC
                        break;
                    case 7: //SAR
                        Registers[destReg] = sar(TempA);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                build_effective_address();

                TempA = parent.ReadWord(overrideSegment, TempC);

                switch ((byte)((tempBL & 0x38) >> 3))
                {
                    case 0: //rol
                        parent.WriteWord(overrideSegment, TempC, rol(TempA));
                        break;
                    case 1: //ror
                        parent.WriteWord(overrideSegment, TempC, ror(TempA));
                        break;
                    case 2: //rcl
                        parent.WriteWord(overrideSegment, TempC, rcl(TempA));
                        break;
                    case 3: //rcr
                        parent.WriteWord(overrideSegment, TempC, rcr(TempA));
                        break;
                    case 4: //sal/shl
                        parent.WriteWord(overrideSegment, TempC, sal(TempA));
                        break;
                    case 5: //shr
                        parent.WriteWord(overrideSegment, TempC, shr(TempA));
                        break;
                    case 6: //NC
                        break;
                    case 7: //SAR
                        parent.WriteWord(overrideSegment, TempC, sar(TempA));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private void rot_shiftn_rm8()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)(tempBL & 0x07);

                tempAL = Registers[destReg];

                switch ((byte)((tempBL & 0x38) >> 3))
                {
                    case 0: //rol
                        Registers[destReg] = rol(tempAL, Registers.CL);
                        break;
                    case 1: //ror
                        Registers[destReg] = ror(tempAL, Registers.CL);
                        break;
                    case 2: //rcl
                        Registers[destReg] = rcl(tempAL, Registers.CL);
                        break;
                    case 3: //rcr
                        Registers[destReg] = rcr(tempAL, Registers.CL);
                        break;
                    case 4: //sal/shl
                        Registers[destReg] = sal(tempAL, Registers.CL);
                        break;
                    case 5: //shr
                        Registers[destReg] = shr(tempAL, Registers.CL);
                        break;
                    case 6: //NC
                        break;
                    case 7: //SAR
                        Registers[destReg] = sar(tempAL, Registers.CL);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                build_effective_address();

                tempAL = parent.ReadByte(overrideSegment, TempC);

                switch ((byte)((tempBL & 0x38) >> 3))
                {
                    case 0: //rol
                        parent.WriteByte(overrideSegment, TempC, rol(tempAL, Registers.CL));
                        break;
                    case 1: //ror
                        parent.WriteByte(overrideSegment, TempC, ror(tempAL, Registers.CL));
                        break;
                    case 2: //rcl
                        parent.WriteByte(overrideSegment, TempC, rcl(tempAL, Registers.CL));
                        break;
                    case 3: //rcr
                        parent.WriteByte(overrideSegment, TempC, rcr(tempAL, Registers.CL));
                        break;
                    case 4: //sal/shl
                        parent.WriteByte(overrideSegment, TempC, sal(tempAL, Registers.CL));
                        break;
                    case 5: //shr
                        parent.WriteByte(overrideSegment, TempC, shr(tempAL, Registers.CL));
                        break;
                    case 6: //NC
                        break;
                    case 7: //SAR
                        parent.WriteByte(overrideSegment, TempC, sar(tempAL, Registers.CL));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private void rot_shiftn_rm16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (WordGeneral)(tempBL & 0x07);

                TempA = Registers[destReg];

                switch ((byte)((tempBL & 0x38) >> 3))
                {
                    case 0: //rol
                        Registers[destReg] = rol(TempA, Registers.CL);
                        break;
                    case 1: //ror
                        Registers[destReg] = ror(TempA, Registers.CL);
                        break;
                    case 2: //rcl
                        Registers[destReg] = rcl(TempA, Registers.CL);
                        break;
                    case 3: //rcr
                        Registers[destReg] = rcr(TempA, Registers.CL);
                        break;
                    case 4: //sal/shl
                        Registers[destReg] = sal(TempA, Registers.CL);
                        break;
                    case 5: //shr
                        Registers[destReg] = shr(TempA, Registers.CL);
                        break;
                    case 6: //NC
                        break;
                    case 7: //SAR
                        Registers[destReg] = sar(TempA, Registers.CL);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                build_effective_address();

                TempA = parent.ReadWord(overrideSegment, TempC);

                switch ((byte)((tempBL & 0x38) >> 3))
                {
                    case 0: //rol
                        parent.WriteWord(overrideSegment, TempC, rol(TempA, Registers.CL));
                        break;
                    case 1: //ror
                        parent.WriteWord(overrideSegment, TempC, ror(TempA, Registers.CL));
                        break;
                    case 2: //rcl
                        parent.WriteWord(overrideSegment, TempC, rcl(TempA, Registers.CL));
                        break;
                    case 3: //rcr
                        parent.WriteWord(overrideSegment, TempC, rcr(TempA, Registers.CL));
                        break;
                    case 4: //sal/shl
                        parent.WriteWord(overrideSegment, TempC, sal(TempA, Registers.CL));
                        break;
                    case 5: //shr
                        parent.WriteWord(overrideSegment, TempC, shr(TempA, Registers.CL));
                        break;
                    case 6: //NC
                        break;
                    case 7: //SAR
                        parent.WriteWord(overrideSegment, TempC, sar(TempA, Registers.CL));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private void aam()
        {
            fetch_next_from_queue();

            tempAL = Registers.AL;

            Registers.AH = (byte)(tempAL / tempBL);

            Registers.AL = (byte)(tempAL % tempBL);

            set_sign(Registers.AL);

            set_parity(Registers.AL);

            flags.ZF = Registers.AL == 0;
        }

        private void aad()
        {
            TempA = Registers.AX;

            Registers.AL = (byte)((tempAL + (tempAH + 0x0a)) & 0xff);

            Registers.AH = 0;

            set_sign(Registers.AL);

            set_parity(Registers.AL);

            flags.ZF = Registers.AL == 0;
        }

        //0xd6 has no documented instruction
#if FULLFEATURE
                private void salc()
                {
                    throw new NotImplementedException();
                }
#endif


        private void xlat()
        {
            tempAL = parent.ReadByte(Segment.DS, (ushort)(Registers.BX + Registers.AL));

            Registers.AL = tempAL;
        }


        // this implementation is based entirely on a the answer to this stackoverflow question;
        // https://stackoverflow.com/questions/42543905/what-are-8086-esc-instruction-opcodes
        private void esc()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                return; // da da da
            }
            else
            {
                build_effective_address();
                return;
            }
        }

        private void loopne_nz_i8()
        {
            Registers.CX--;
            if (Registers.CX == 0)
                return;

            fetch_next_from_queue();

            if (!flags.ZF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void loope_z_i8()
        {
            Registers.CX--;
            if (Registers.CX == 0)
                return;

            fetch_next_from_queue();

            if (flags.ZF)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void loop_i8()
        {
            Registers.CX--;
            if (Registers.CX == 0)
                return;

            fetch_next_from_queue();

            parent.JumpShort(tempBL);
        }

        private void jcxz_i8()
        {
            fetch_next_from_queue();

            if (Registers.CX == 0)
            {
                parent.JumpShort(tempBL);
            }
        }

        private void inb_i8()
        {
            fetch_next_from_queue();
            Registers.AL = parent.InByte(tempBL);
        }

        private void inw_i8()
        {
            fetch_next_from_queue();
            Registers.AX = parent.InWord(tempBL);
        }

        private void outb_i8()
        {
            fetch_next_from_queue();
            parent.OutByte(tempBL, Registers.AL);
        }

        private void outw_i8()
        {
            fetch_next_from_queue();
            parent.OutWord(tempBL, Registers.AX);
        }

        private void call_near_i16()
        {
            parent.WriteIPToStack(Registers.SP);
            Registers.SP -= 2;

            //get the offset
            fetch_next_from_queue();
            tempAL = tempBL;
            fetch_next_from_queue();
            tempAH = tempBL;

            parent.JumpNear(TempA);
        }

        private void jmp_near_i16()
        {
            fetch_next_from_queue();
            tempAL = tempBL;
            fetch_next_from_queue();
            tempAH = tempBL;

            parent.JumpNear(TempA);
        }

        private void jmp_far_i16_i16()
        {
            // offset
            fetch_next_from_queue();
            tempAL = tempBL;
            fetch_next_from_queue();
            tempAH = tempBL;

            //segment
            fetch_next_from_queue();
            tempCL = tempBL;
            fetch_next_from_queue();
            tempCH = tempBL;

            parent.JumpFar(TempC, TempA);
        }

        private void jmp_short_i8()
        {
            fetch_next_from_queue();

            parent.JumpShort(tempBL);
        }

        private void inb_dx()
        {
            Registers.AL = parent.InByte(Registers.DX);
        }

        private void inw_dx()
        {
            Registers.AX = parent.InWord(Registers.DX);
        }

        private void outb_dx()
        {
            parent.OutByte(Registers.DX, Registers.AL);
        }

        private void outw_dx()
        {
            parent.OutWord(Registers.DX, Registers.AX);
        }

        private void lck()
        {
            parent.AssertLock();
            fetch_next_from_queue();
            instructions[tempBL]?.Invoke();
            parent.DeassertLock();
        }

        //0xF1 has no instruction


        // Because of the way we expect hardware interrupts to work, and the way the intel documentation
        // says interrupts work during rep instructions, rep instructions need to run the show until they're complete
        // and do their own checking for hardward interrupts
        private void repne_nz()
        {
            //Check for pending interrupt

            fetch_next_from_queue();

            // cmps, scas
            if (tempBL == 0xa6 || tempBL == 0xa7 || tempBL == 0xae || tempBL == 0xaf)
            {
                repInstruction = true;
                instructions[tempBL].Invoke();
                Registers.CX--;
                if (Registers.CX == 0 || flags.ZF)
                {
                    repInstruction = false;
                    return;
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private void rep_e_nz()
        {
            if (tempBL == 0xa4 || tempBL == 0xa5 || tempBL == 0xaa || tempBL == 0xab || tempBL == 0xac || tempBL == 0xad)
            {
                repInstruction = true;
                instructions[tempBL].Invoke();
                Registers.CX--;
                if (Registers.CX == 0 || !flags.ZF)
                {
                    repInstruction = false;
                    return;
                }
            }
        }

        private void halt()
        {
            throw new NotImplementedException();
        }

        private void cmc()
        {
            flags.CF ^= true;
        }

        private void oxf6_rm8()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)(tempBL & 0x07);
                tempAL = Registers[destReg];
            }
            else
            {
                build_effective_address();
                tempAL = parent.ReadByte(overrideSegment, TempC);
            }
            switch ((byte)((tempBL & 0x38) >> 3))
            {
                case 0: // test_rm8_imm8
                    fetch_next_from_queue();
                    set_flags_and_and(tempAL, tempBL);
                    return;
                case 1: // nc
                    return;
                case 2: // not_rm8
                    Registers.AL = (byte)(tempCL ^ 0xff);
                    return;
                case 3: // neg_rm8
                    if (tempAL == 0x80)
                    {
                        flags.OF = true;
                        return;
                    }
                    Registers.AL = set_flags_and_diff(0, tempAL);
                    return;
                case 4: // mul_rm8
                    Registers.AX = (ushort)(Registers.AL * tempAL);
                    flags.CF = flags.OF = Registers.AH != 0;
                    return;
                case 5: // imul_rm8
                    Registers.AX = set_flags_and_imul(Registers.AL, tempAL);
                    return;
                case 6: // div_rm8
                    if (Registers.AX / tempAL > 0xff)
                    {
                        //INT 0
                    }
                    tempCL = (byte)(Registers.AX / tempAL);
                    tempCH = (byte)(Registers.AX % tempAL);
                    Registers.AX = TempC;
                    return;
                case 7: // idiv_rm8
                    if ((short)Registers.AX / (sbyte)tempAL > 127 || (short)Registers.AX / (sbyte)tempAL < -127)
                    {
                        //INT 0
                    }
                    tempCL = (byte)((short)Registers.AX / (sbyte)tempAL);
                    tempCH = (byte)((short)Registers.AX % (sbyte)tempAL);
                    Registers.AX = TempC;
                    return;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void oxf7_rm16()
        {
            fetch_next_from_queue();
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (WordGeneral)(tempBL & 0x07);
                TempA = Registers[destReg];
            }
            else
            {
                build_effective_address();
                TempA = parent.ReadWord(overrideSegment, TempC);
            }
            switch ((byte)((tempBL & 0x38) >> 3))
            {
                case 0: // test_rm16_imm16
                    fetch_next_from_queue();
                    tempCL = tempBL;
                    fetch_next_from_queue();
                    tempCH = tempBL;
                    set_flags_and_and(TempA, TempC);
                    return;
                case 1: // nc
                    return;
                case 2: // not_rm16
                    Registers.AX = (ushort)(TempC ^ 0xffff);
                    return;
                case 3: // neg_rm16
                    if (TempA == 0x8000)
                    {
                        flags.OF = true;
                        return;
                    }
                    Registers.AX = set_flags_and_diff(0, TempA);
                    return;
                case 4: // mul_rm16
                    uint result = (uint)Registers.AX * TempA;
                    flags.CF = flags.OF = (result & 0xffff0000) != 0;
                    Registers.DX = (ushort)((result & 0xffff0000) >> 16);
                    Registers.AX = (ushort)(result & 0xffff);
                    return;
                case 5: // imul_rm16
                    int iresult = Registers.AX * TempA;
                    uint signtest = (ushort)(iresult & 0xffff);
                    signtest |= (((signtest & 0x8000) != 0) ? 0xffff0000 : 0);
                    flags.CF = flags.OF = iresult == signtest;
                    Registers.DX = (ushort)((iresult & 0xffff0000) >> 16);
                    Registers.AX = (ushort)(iresult & 0xffff);
                    break;
                case 6: // div_rm8
                    uint operand = (uint)((Registers.DX << 16) | Registers.AX);
                    if (operand / TempB > 0xffff)
                    {
                        //INT 0
                    }
                    Registers.DX = (ushort)(operand % TempA);
                    Registers.AX = (ushort)(operand / TempA);
                    return;
                case 7: // idiv_rm8
                    int ioperand = (Registers.DX << 16) | Registers.AX;
                    if (ioperand / TempA > 32767 || ioperand / TempA > -32767)
                    {
                        //INT 0
                    }
                    Registers.DX = (ushort)(ioperand % TempA);
                    Registers.AX = (ushort)(ioperand / TempA);
                    return;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void clc()
        {
            flags.CF = false;
        }

        private void stc()
        {
            flags.CF = true;
        }

        private void cli()
        {
            flags.IF = false;
        }

        // 
        private void sti()
        {
            zeroize_temps();
            Opcode = parent.GetNextFromQueue();
            instructions[Opcode]?.Invoke();
            flags.IF = true;
        }

        private void cld()
        {
            flags.DF = false;
        }

        private void std()
        {
            flags.DF = true;
        }

        private void inc_dec_rm8()
        {
            fetch_next_from_queue();
            var op = (byte)((tempBL & 0x38) >> 3);
            if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
            {
                var destReg = (ByteGeneral)(tempBL & 0x07);
                Registers[destReg] = op == 0 ? (byte)(Registers[destReg] + 1) : (byte)(Registers[destReg] - 1);
            }
            else
            {
                build_effective_address();
                var dest = parent.ReadByte(overrideSegment, TempC);
                dest = op == 0 ? (byte)(dest + 1) : (byte)(dest - 1);
                parent.WriteByte(overrideSegment, TempC, dest);
            }
        }

        private void oxff()
        {
            fetch_next_from_queue();
            var op = (byte)((tempBL & 0x38) >> 3);
            switch (op)
            {
                case 0:
                    build_effective_address();
                    TempA = parent.ReadWord(overrideSegment, TempC);
                    TempA++;
                    parent.WriteWord(overrideSegment, TempC, TempA);
                    return;
                case 1:
                    build_effective_address();
                    TempA = parent.ReadWord(overrideSegment, TempC);
                    TempA--;
                    parent.WriteWord(overrideSegment, TempC, TempA);
                    return;
                case 2:
                    if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                    {
                        var destReg = (WordGeneral)(tempBL & 0x07);
                        TempC = Registers[destReg];
                    }
                    else
                    {
                        build_effective_address();
                    }
                    parent.WriteIPToStack(Registers.SP);
                    Registers.SP -= 2;
                    parent.JumpNear(TempC);
                    return;
                case 3:
                    build_effective_address();
                    TempA = parent.ReadWord(overrideSegment, TempC);
                    TempB = parent.ReadWord(overrideSegment, (ushort)(TempC + 2));
                    push_cs();
                    parent.WriteIPToStack(Registers.SP);
                    Registers.SP -= 2;
                    parent.JumpFar(TempB, TempA);
                    return;
                case 4:
                    if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                    {
                        var destReg = (WordGeneral)(tempBL & 0x07);
                        TempC = Registers[destReg];
                    }
                    else
                    {
                        build_effective_address();
                    }
                    parent.JumpNear(TempC);
                    return;
                case 5:
                    build_effective_address();
                    TempA = parent.ReadWord(overrideSegment, TempC);
                    TempB = parent.ReadWord(overrideSegment, (ushort)(TempC + 2));
                    parent.JumpFar(TempB, TempA);
                    return;
                case 6:
                    build_effective_address();
                    TempA = parent.ReadWord(overrideSegment, TempC);
                    parent.WriteWord(Segment.SS, Registers.SP, TempA);
                    Registers.SP -= 2;
                    return;
                default:
                    throw new InvalidOperationException();
            }
        }

        // Helper methods
        /// <summary>
        /// Builds the effective address for the current operation based on mod/rm and any relevant immediate values and stores it in TempC
        /// </summary>
        /// <remarks>
        /// Assumes that TempB already contains the ModRM byte and that any memory values are still in the instruction queue. Does not modify tempBL
        /// </remarks>
        private void build_effective_address()
        {
            ModEncoding mod = (ModEncoding)((tempBL & 0xC0) >> 6);
            RmEncoding rm = (RmEncoding)(tempBL & 0x07);

            switch (mod)
            {
                case ModEncoding.registerDisplacement:
                    if (rm == RmEncoding.BP)
                    {
                        fetch_next_from_queue();
                        tempCL = tempBL;
                        fetch_next_from_queue();
                        tempCH = tempBL;
                    }
                    else
                    {
                        TempC = 0;
                    }
                    break;
                case ModEncoding.byteDisplacement:
                    fetch_next_from_queue();
                    tempCL = tempBL;
                    break;
                case ModEncoding.wordDisplacement:
                    fetch_next_from_queue();
                    tempCL = tempBL;
                    fetch_next_from_queue();
                    tempCH = tempBL;
                    break;
                case ModEncoding.registerRegister:
                    TempC = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (rm)
            {
                case RmEncoding.BXSI:
                    TempC += Registers.BX;
                    TempC += Registers.SI;
                    break;
                case RmEncoding.BXDI:
                    TempC += Registers.BX;
                    TempC += Registers.DI;
                    break;
                case RmEncoding.BPSI:
                    TempC += Registers.BP;
                    TempC += Registers.SI;
                    break;
                case RmEncoding.BPDI:
                    TempC += Registers.BP;
                    TempC += Registers.DI;
                    break;
                case RmEncoding.SI:
                    TempC += Registers.SI;
                    break;
                case RmEncoding.DI:
                    TempC += Registers.DI;
                    break;
                case RmEncoding.BP:
                    if (mod != ModEncoding.registerDisplacement)
                    {
                        TempC += Registers.BP;
                    }
                    break;
                case RmEncoding.BX:
                    TempC += Registers.BX;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Retrieves the next byte from the instruction queue and places it in tempBL
        /// </summary>
        /// <remarks>
        /// Blocks if no byte is available
        /// </remarks>
        private void fetch_next_from_queue()
        {
            tempBL = parent.GetNextFromQueue();
        }

        /// <summary>
        /// Asks the BIU to write the byte in tempAL to the memory address pointed at by tempC
        /// tempBL contains segment override information if applicable
        /// </summary>
        private void byte_to_memory()
        {
            parent.WriteByte(overrideSegment, TempC, tempAL);
        }

        /// <summary>
        /// Asks the BIU to write the byte in tempA to the memory address pointed at by tempC
        /// tempBL contains segment override information if applicable
        /// </summary>
        private void word_to_memory()
        {
            //busInterfaceUnit.SetWord(overrideSegment, TempC, TempA);
        }

        /// <summary>
        /// Asks the BIU to retrieve a byte from the memory address pointed at by tempC
        /// byte is placed in TempBL
        /// </summary>
        private void byte_from_memory()
        {
            tempAL = parent.ReadByte(overrideSegment, TempC);
        }

        /// <summary>
        /// Asks the BIU to retrieve a word from the memory address pointed at by tempC
        /// word is placed in TempB
        /// </summary>
        private void word_from_memory()
        {
            TempA = parent.ReadByte(overrideSegment, TempC);
        }

        /// <summary>
        /// Tells the BIU to increment the IP by tempCL
        /// </summary>
        private void jump_short()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tells the BIU to change the IP to tempC
        /// </summary>
        private void jump_near()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tells the BIU to change the CS to TempB and the IP to TempC
        /// </summary>
        private void jump_far()
        {
            throw new NotImplementedException();
        }

        private void set_sign(byte value)
        {
            flags.SF = (value & 128) != 0;
        }

        private void set_sign(ushort value)
        {
            flags.SF = (value & 32768) != 0;
        }

        private void set_parity(byte value)
        {
            flags.PF = true;
            for (int i = 0; i < 8; i++)
            {
                flags.PF ^= (value & (1 << i)) != 0;
            }
        }

        private void set_parity(ushort value)
        {
            flags.PF = true;
            for (int i = 0; i < 16; i++)
            {
                flags.PF ^= (value & (1 << i)) != 0;
            }
        }

        private byte set_flags_and_sum(byte dest, byte src)
        {
            ushort sum = (ushort)(dest + src);

            flags.AF = (dest & 0x0f) + (src & 0x0f) > 0x0f;

            flags.CF = sum > 0xff;
            sum &= 0xff;

            flags.OF = ((dest < 0x80) || (src < 0x80)) && (sum >= 0x80);
            flags.ZF = sum == 0;
            set_sign((byte)sum);
            set_parity((byte)sum);
            return (byte)sum;
        }

        private ushort set_flags_and_sum(ushort dest, ushort src)
        {
            uint sum = (uint)(dest + src);

            flags.AF = (dest & 0x0f) + (src & 0x0f) > 0x0f;


            flags.CF = sum > 0xffff;
            sum &= 0xffff;

            flags.OF = ((dest < 0x8000) || (src < 0x8000)) && (sum >= 0x8000);
            flags.ZF = sum == 0;
            set_sign((ushort)sum);
            set_parity((ushort)sum);
            return (ushort)sum;
        }

        private byte set_flags_and_sum_carry(byte dest, byte src)
        {
            var carry = flags.CF ? 1 : 0;

            return (byte)(set_flags_and_sum(dest, src) + carry);
        }

        private ushort set_flags_and_sum_carry(ushort dest, ushort src)
        {
            var carry = flags.CF ? 1 : 0;
            return (ushort)(set_flags_and_sum(dest, src) + carry);
        }

        private byte set_flags_and_or(byte dest, byte src)
        {
            byte result = (byte)(dest | src);
            flags.OF = false;
            flags.CF = false;
            flags.ZF = result == 0;
            set_sign(result);
            set_parity(result);

            return result;
        }

        private ushort set_flags_and_or(ushort dest, ushort src)
        {
            ushort result = (ushort)(dest | src);
            flags.OF = false;
            flags.CF = false;
            flags.ZF = result == 0;
            set_sign(result);
            set_parity(result);

            return result;
        }

        private byte set_flags_and_diff(byte dest, byte src)
        {
            byte diff = (byte)(dest - src);

            flags.AF = (dest & 0x0f) > (src & 0x0f);

            flags.CF = dest > src;

            flags.OF = (dest - src) < -128;

            flags.ZF = diff == 0;
            set_sign(diff);
            set_parity(diff);

            return diff;
        }

        private ushort set_flags_and_diff(ushort dest, ushort src)
        {
            ushort diff = (ushort)(dest - src);

            flags.AF = (dest & 0x0f) > (src & 0x0f);

            flags.CF = dest > src;

            flags.OF = (dest - src) < -32768;

            flags.ZF = diff == 0;

            set_sign(diff);
            set_parity(diff);

            return diff;
        }

        private byte set_flags_and_diff_borrow(byte dest, byte src)
        {
            var carry = flags.CF ? -1 : 0;

            return (byte)(set_flags_and_diff(dest, src) + carry);
        }

        private ushort set_flags_and_diff_borrow(ushort dest, ushort src)
        {
            var carry = flags.CF ? -1 : 0;

            return (ushort)(set_flags_and_diff(dest, src) + carry);
        }

        private byte set_flags_and_and(byte dest, byte src)
        {
            byte result = (byte)(dest & src);
            flags.OF = false;
            flags.CF = false;
            flags.ZF = result == 0;
            set_sign(result);
            set_parity(result);

            return result;
        }

        private ushort set_flags_and_and(ushort dest, ushort src)
        {
            ushort result = (ushort)(dest & src);
            flags.OF = false;
            flags.CF = false;
            flags.ZF = result == 0;
            set_sign(result);
            set_parity(result);

            return result;
        }

        private byte set_flags_and_xor(byte dest, byte src)
        {
            byte result = (byte)(dest ^ src);
            flags.OF = false;
            flags.CF = false;
            flags.ZF = result == 0;
            set_sign(result);
            set_parity(result);

            return result;
        }

        private ushort set_flags_and_xor(ushort dest, ushort src)
        {
            ushort result = (ushort)(dest ^ src);
            flags.OF = false;
            flags.CF = false;
            flags.ZF = result == 0;
            set_sign(result);
            set_parity(result);

            return result;
        }

        private ushort set_flags_and_imul(byte dest, byte src)
        {
            short result = 0;
            result = (short)(dest * src);
            ushort signtest = (ushort)(result & 0xff);
            signtest |= (ushort)(((signtest & 0x80) != 0) ? 0xff00 : 0);
            flags.CF = flags.OF = result == signtest;
            return (ushort)result;
        }

        private byte rol(byte value, byte count = 1)
        {
            bool sign = (value & 0x80) != 0;

            byte mask = (byte)((0xff - ((1 << 8 - count) - 1)) & value);

            mask >>= 8 - count;

            value <<= count;

            value |= mask;

            flags.CF = (value & 0x01) != 0;

            flags.OF = (count == 1) && (sign != ((value & 0x80) != 0));

            return value;
        }

        private byte ror(byte value, byte count = 1)
        {
            bool sign = (value & 0x80) != 0;

            byte mask = (byte)(((1 << count) - 1) & value);

            mask <<= 8 - count;

            value >>= count;

            value |= mask;

            flags.CF = (value & 0x80) != 0;

            flags.OF = (count == 1) && (sign != ((value & 0x80) != 0));

            return value;
        }

        private byte rcl(byte value, byte count = 1)
        {
            bool sign = (value & 0x80) != 0;

            byte mask = (byte)((0xff - ((1 << 8 - count) - 1)) & value);

            mask >>= 1;

            mask |= flags.CF ? 0x80 : 0;

            mask >>= 8 - count - 1;

            flags.CF = (mask & 0x01) != 0;

            mask >>= 1;

            value <<= count;

            value |= mask;

            flags.OF = (count == 1) && (sign != ((value & 0x80) != 0));

            return value;
        }

        private byte rcr(byte value, byte count = 1)
        {
            bool sign = (value & 0x80) != 0;

            byte mask = (byte)(((1 << count) - 1) & value);

            mask <<= 1;

            mask |= flags.CF ? 0x01 : 0;

            mask <<= 8 - count - 1;

            flags.CF = (mask & 0x80) != 0;

            mask <<= 1;

            value >>= count;

            value |= mask;

            flags.OF = (count == 1) && (sign != ((value & 0x80) != 0));

            return value;
        }

        private byte sal(byte value, byte count = 1)
        {
            bool sign = (value & 0x80) != 0;

            flags.CF = (value & (1 << 8 - count)) != 0;

            value <<= count;

            flags.SF = (value & 0x80) != 0;

            flags.OF = sign != flags.SF;

            flags.ZF = value == 0;

            set_parity(value);

            return value;
        }

        private byte shr(byte value, byte count = 1)
        {
            bool sign = (value & 0x80) != 0;

            flags.SF = false;

            flags.CF = (value & (1 << count - 1)) != 0;

            value >>= count;

            flags.OF = sign;

            flags.ZF = value == 0;

            set_parity(value);

            return value;
        }

        private byte sar(byte value, byte count = 1)
        {
            flags.SF = (value & 0x80) != 0;

            flags.CF = (value & (1 << count - 1)) != 0;

            value >>= count;

            byte mask = (byte)(0xff - ((1 << 8 - count) - 1));

            value |= flags.SF ? mask : 0;

            flags.OF = false;

            flags.ZF = value == 0;

            set_parity(value);

            return value;
        }

        private ushort rol(ushort value, byte count = 1)
        {
            bool sign = (value & 0x8000) != 0;

            ushort mask = (ushort)((0xffff - ((1 << 16 - count) - 1)) & value);

            mask >>= 16 - count;

            value <<= count;

            value |= mask;

            flags.CF = (value & 0x01) != 0;

            flags.OF = (count == 1) && (sign != ((value & 0x8000) != 0));

            return value;
        }

        private ushort ror(ushort value, byte count = 1)
        {
            bool sign = (value & 0x8000) != 0;

            ushort mask = (ushort)(((1 << count) - 1) & value);

            mask <<= 16 - count;

            value >>= count;

            value |= mask;

            flags.CF = (value & 0x8000) != 0;

            flags.OF = (count == 1) && (sign != ((value & 0x8000) != 0));

            return value;
        }

        private ushort rcl(ushort value, byte count = 1)
        {
            bool sign = (value & 0x8000) != 0;

            ushort mask = (ushort)((0xffff - ((1 << 16 - count) - 1)) & value);

            mask >>= 1;

            mask |= flags.CF ? 0x8000 : 0;

            mask >>= 16 - count - 1;

            flags.CF = (mask & 0x01) != 0;

            mask >>= 1;

            value <<= count;

            value |= mask;

            flags.OF = (count == 1) && (sign != ((value & 0x8000) != 0));

            return value;
        }

        private ushort rcr(ushort value, byte count = 1)
        {
            bool sign = (value & 0x8000) != 0;

            ushort mask = (ushort)(((1 << count) - 1) & value);

            mask <<= 1;

            mask |= flags.CF ? 0x01 : 0;

            mask <<= 16 - count - 1;

            flags.CF = (mask & 0x8000) != 0;

            mask <<= 1;

            value >>= count;

            value |= mask;

            flags.OF = (count == 1) && (sign != ((value & 0x8000) != 0));

            return value;
        }

        private ushort sal(ushort value, byte count = 1)
        {
            bool sign = (value & 0x8000) != 0;

            flags.CF = (value & (1 << 16 - count)) != 0;

            value <<= count;

            flags.SF = (value & 0x8000) != 0;

            flags.OF = sign != flags.SF;

            flags.ZF = value == 0;

            set_parity(value);

            return value;
        }

        private ushort shr(ushort value, byte count = 1)
        {
            bool sign = (value & 0x8000) != 0;

            flags.SF = false;

            flags.CF = (value & (1 << count - 1)) != 0;

            value >>= count;

            flags.OF = sign;

            flags.ZF = value == 0;

            set_parity(value);

            return value;
        }

        private ushort sar(ushort value, byte count = 1)
        {
            flags.SF = (value & 0x8000) != 0;

            flags.CF = (value & (1 << count - 1)) != 0;

            value >>= count;

            ushort mask = (byte)(0xffff - ((1 << 16 - count) - 1));

            value |= flags.SF ? mask : 0;

            flags.OF = false;

            flags.ZF = value == 0;

            set_parity(value);

            return value;
        }

    }
}
