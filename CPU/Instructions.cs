using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU
{
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
     *      
     *      Tedious examination of assembled simple code
     */
    public partial class ExecutionUnit
    {
        private Action[] instructionSet;
        private readonly bool[] needModRm = {
           //   0     1     2     3     4     5     6     7     8     9     a     b     c     d     e     f
                true, true, true, true, false,false,false,false,true, true, true, true, false,false,false,false, // 0x00 - 0x0f
                true, true, true, true, false,false,false,false,true, true, true, true, false,false,false,false, // 0x10 - 0x1f
                true, true, true, true, false,false,false,false,true, true, true, true, false,false,false,false, // 0x20 - 0x2f
                true, true, true, true, false,false,false,false,true, true, true, true, false,false,false,false, // 0x30 - 0x3f
                false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false, // 0x40 - 0x4f
                false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false, // 0x50 - 0x5f
                false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false, // 0x60 - 0x6f
                false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false, // 0x70 - 0x7f
                true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, // 0x80 - 0x8f
                false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false, // 0x90 - 0x9f
                false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false, // 0xa0 - 0xaf
                false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false, // 0xb0 - 0xbf
                false,false,false,false,true, true, true, true, false,false,false,false,false,false,false,false, // 0xc0 - 0xcf
                true, true, true, true, false,false,false,false,true, true, true, true, true, true, true, true,  // 0xd0 - 0xdf
                false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false, // 0xe0 - 0xef
                false,false,false,false,false,false,true, true, false,false,false,false,false,true, false,false  // 0xf0 - 0xff
            };

        private void initialize_instruction_set()
        {
            instructionSet = new Action[256];

            // [opcode] = function_ptr   // instr   dest,           src             [modrm]

            instructionSet[0x00] = null; // add     reg8/mem8       reg8            [true]
            instructionSet[0x01] = null; // add     reg16/mem16     reg16           [true]
            instructionSet[0x02] = null; // add     reg8            reg8/mem8       [true]
            instructionSet[0x03] = null; // add     reg16           reg16/mem16     [true]
            instructionSet[0x04] = null; // add     al              imm8            [false]
            instructionSet[0x05] = null; // add     ax              imm16           [false]

            instructionSet[0x06] = null; // push    es                              [false]
            instructionSet[0x07] = null; // pop     es                              [false]

            instructionSet[0x08] = null; // or      reg8/mem8       reg8            [true]
            instructionSet[0x09] = null; // or      reg16/mem16     reg16           [true]
            instructionSet[0x0a] = null; // or      reg8            reg8/mem8       [true]
            instructionSet[0x0b] = null; // or      reg16           reg16/mem16     [true]
            instructionSet[0x0c] = null; // or      al              imm8            [false]
            instructionSet[0x0d] = null; // or      ax              imm16           [false]

            instructionSet[0x0e] = null; // push    cs                              [false]

            instructionSet[0x0f] = null; //NC (pop cs)

            instructionSet[0x10] = null; // adc     reg8/mem8       reg8            [true]
            instructionSet[0x11] = null; // adc     reg16/mem16     reg16           [true]
            instructionSet[0x12] = null; // adc     reg8            reg8/mem8       [true]
            instructionSet[0x13] = null; // adc     reg16           reg16/mem16     [true]
            instructionSet[0x14] = null; // adc     imm8                            [false]
            instructionSet[0x15] = null; // adc     imm16                           [false]

            instructionSet[0x16] = null; // push    ss                              [false]
            instructionSet[0x17] = null; // pop     ss                              [false]

            instructionSet[0x18] = null; // sbb     reg8/mem8       reg8            [true]
            instructionSet[0x19] = null; // sbb     reg16/mem16     reg16           [true]
            instructionSet[0x1a] = null; // sbb     reg8            reg8/mem8       [true]
            instructionSet[0x1b] = null; // sbb     reg16           reg16/mem16     [true]
            instructionSet[0x1c] = null; // sbb     imm8                            [false]
            instructionSet[0x1d] = null; // sbb     imm16                           [false]

            instructionSet[0x1e] = null; // push    ds                              [false]
            instructionSet[0x1f] = null; // pop     ds                              [false]

            instructionSet[0x20] = null; // and     reg8/mem8       reg8            [true]
            instructionSet[0x21] = null; // and     reg16/mem16     reg16           [true]
            instructionSet[0x22] = null; // and     reg8            reg8/mem8       [true]
            instructionSet[0x23] = null; // and     reg16           reg16/mem16     [true]
            instructionSet[0x24] = null; // and     imm8                            [false]
            instructionSet[0x25] = null; // and     imm16                           [false]

            instructionSet[0x26] = null; // (segment override - es)                 [false]
            instructionSet[0x27] = null; // daa                                     [false]

            instructionSet[0x28] = null; // sub     reg8/mem8       reg8            [true]
            instructionSet[0x29] = null; // sub     reg16/mem16     reg16           [true]
            instructionSet[0x2a] = null; // sub     reg8            reg8/mem8       [true]
            instructionSet[0x2b] = null; // sub     reg16           reg16/mem16     [true]
            instructionSet[0x2c] = null; // sub     imm8                            [false]
            instructionSet[0x2d] = null; // sub     imm16                           [false]

            instructionSet[0x2e] = null; // (segment override - cs)                 [false]
            instructionSet[0x2f] = null; // das                                     [false]

            instructionSet[0x30] = null; // xor     reg8/mem8       reg8            [true]
            instructionSet[0x31] = null; // xor     reg16/mem16     reg16           [true]
            instructionSet[0x32] = null; // xor     reg8            reg8/mem8       [true]
            instructionSet[0x33] = null; // xor     reg16           reg16/mem16     [true]
            instructionSet[0x34] = null; // xor     imm8                            [false]
            instructionSet[0x35] = null; // xor     imm16                           [false]

            instructionSet[0x36] = null; // (segment override - ss)                 [false]
            instructionSet[0x37] = null; // aaa                                     [false]

            instructionSet[0x38] = null; // cmp     reg8/mem8       reg8            [true]
            instructionSet[0x39] = null; // cmp     reg16/mem16     reg16           [true]
            instructionSet[0x3a] = null; // cmp     reg8            reg8/mem8       [true]
            instructionSet[0x3b] = null; // cmp     reg16           reg16/mem16     [true]
            instructionSet[0x3c] = null; // cmp     imm8                            [false]
            instructionSet[0x3d] = null; // cmp     imm16                           [false]

            instructionSet[0x3e] = null; // (segment override - ds)
            instructionSet[0x3f] = null; // aas

            instructionSet[0x40] = null; // inc ax
            instructionSet[0x41] = null; // inc cx
            instructionSet[0x42] = null; // inc dx
            instructionSet[0x43] = null; // inc bx
            instructionSet[0x44] = null; // inc sp
            instructionSet[0x45] = null; // inc bp
            instructionSet[0x46] = null; // inc si
            instructionSet[0x47] = null; // inc di
            instructionSet[0x48] = null; // dec ax
            instructionSet[0x49] = null; // dec cx
            instructionSet[0x4a] = null; // dec dx
            instructionSet[0x4b] = null; // dec bx
            instructionSet[0x4c] = null; // dec sp
            instructionSet[0x4d] = null; // dec bp
            instructionSet[0x4e] = null; // dec si
            instructionSet[0x4f] = null; // dec di

            instructionSet[0x50] = null; // push ax
            instructionSet[0x51] = null; // push cx
            instructionSet[0x52] = null; // push dx
            instructionSet[0x53] = null; // push bx
            instructionSet[0x54] = null; // push sp
            instructionSet[0x55] = null; // push bp
            instructionSet[0x56] = null; // push si
            instructionSet[0x57] = null; // push di
            instructionSet[0x58] = null; // pop ax
            instructionSet[0x59] = null; // pop cx
            instructionSet[0x5a] = null; // pop dx
            instructionSet[0x5b] = null; // pop bx
            instructionSet[0x5c] = null; // pop sp
            instructionSet[0x5d] = null; // pop bp
            instructionSet[0x5e] = null; // pop si
            instructionSet[0x5f] = null; // pop di

            instructionSet[0x60] = null; // NC
            instructionSet[0x61] = null; // NC
            instructionSet[0x62] = null; // NC
            instructionSet[0x63] = null; // NC
            instructionSet[0x64] = null; // NC
            instructionSet[0x65] = null; // NC
            instructionSet[0x66] = null; // NC
            instructionSet[0x67] = null; // NC
            instructionSet[0x68] = null; // NC
            instructionSet[0x69] = null; // NC
            instructionSet[0x6a] = null; // NC
            instructionSet[0x6b] = null; // NC
            instructionSet[0x6c] = null; // NC
            instructionSet[0x6d] = null; // NC
            instructionSet[0x6e] = null; // NC
            instructionSet[0x6f] = null; // NC

            //Short jumps (all to inc8)
            instructionSet[0x70] = null; // jo
            instructionSet[0x71] = null; // jno
            instructionSet[0x72] = null; // jb/jnae/jc
            instructionSet[0x73] = null; // jnb/jae/jnc
            instructionSet[0x74] = null; // je/jz
            instructionSet[0x75] = null; // jne/jnz
            instructionSet[0x76] = null; // jbe/jna
            instructionSet[0x77] = null; // jnbe/ja
            instructionSet[0x78] = null; // js
            instructionSet[0x79] = null; // jns
            instructionSet[0x7a] = null; // jp/jpe
            instructionSet[0x7b] = null; // jnp/jpo
            instructionSet[0x7c] = null; // jl/jnge
            instructionSet[0x7d] = null; // jnl/jge
            instructionSet[0x7e] = null; // jle/jng
            instructionSet[0x7f] = null; // jnle/jg

            //Intel's ridiculous multiplexed instructions. there are entire 16-instruction empty blocks
            //which instruction is determined by bits 4-6 of byte 2:
            //000 - add, 001 - or, 010 - adc, 011 - sbb, 100 - and, 101 - sub, 110 - xor, 111 - cmp
            instructionSet[0x80] = null; // reg8/mem8, imm8 [true]
            instructionSet[0x81] = null; // reg16/mem16, imm16 [true]
            //000 - add, 001 - NC, 010 - adc, 011 - sbb, 100 - NC, 101 - sub, 110 - NC, 111 - cmp
            instructionSet[0x82] = null; // reg8/mem8, imm8 [true]
            instructionSet[0x83] = null; // reg16/mem16, imm16 [true]

            instructionSet[0x84] = null; // test reg8/mem8, reg8 [true]
            instructionSet[0x85] = null; // test reg16/mem16, reg16 [true]
            instructionSet[0x86] = null; // xchg reg8, reg8/mem8 [true]
            instructionSet[0x87] = null; // xchg reg16, reg16/mem16 [true]
            instructionSet[0x88] = null; // mov reg8/mem8, reg8 [true]
            instructionSet[0x89] = null; // mov reg16/mem16, reg16 [true]
            instructionSet[0x8a] = null; // mov reg8, reg8/mem8 [true]
            instructionSet[0x8b] = null; // mov reg16, reg16/mem16 [true]
            instructionSet[0x8c] = null; // mov reg16/mem16, seg [true]
            instructionSet[0x8d] = null; // lea reg16, mem16 [true]
            instructionSet[0x8e] = null; // mov seg, reg16/mem16 [true]
            instructionSet[0x8f] = null; // pop reg16/mem16

            instructionSet[0x90] = null; // NOP (xchg ax,ax)  [false]
            instructionSet[0x91] = null; // xchg ax,cx [false]
            instructionSet[0x92] = null; // xchg ax,dx [false]
            instructionSet[0x93] = null; // xchg ax,bx [false]
            instructionSet[0x94] = null; // xchg ax,sp [false]
            instructionSet[0x95] = null; // xchg ax,bp [false]
            instructionSet[0x96] = null; // xchg ax,si [false]
            instructionSet[0x97] = null; // xchg ax,di [false]

            instructionSet[0x98] = null; // cbw [false] (sign-extend AL into AH)
            instructionSet[0x99] = null; // cwd [false] (sign-extend AX into DX)

            instructionSet[0x9a] = null; // call disp-lo, disp-hi, seg-lo, seg-hi (assumedly sets CS to seg-hi:seg-lo and IP to disp-hi:disp-lo)

            instructionSet[0x9b] = null; // wait (x87 stuff?)

            //push/pop flags transfer all flag bits
            instructionSet[0x9c] = null; // pushf (push flags)
            instructionSet[0x9d] = null; // popf (pop flags)

            //sahf/lahf only transfer bits 7,6,4,2,0 (SF,ZF,AF,PF,CF)
            instructionSet[0x9e] = null; // sahf (ah -> flags)
            instructionSet[0x9f] = null; // lahf (flags -> ah)

            instructionSet[0xa0] = null; // mov al, mem8 [false]
            instructionSet[0xa1] = null; // mov ax, mem16 [false]
            instructionSet[0xa2] = null; // mov mem8, al [false]
            instructionSet[0xa3] = null; // mov mem16, ax [false]

            //array functions (intel calls them string functions)
            instructionSet[0xa4] = null; // movsb (copy byte at ds:si to es:di, inc/dec si/di according to DF)
            instructionSet[0xa5] = null; // movsw (copy word at ds:si to es:di, inc/dec si/di according to DF)
            instructionSet[0xa6] = null; // cmpsb (compare byte at ds:si to es:di, inc/dec si/di according to DF)
            instructionSet[0xa7] = null; // cmpsw (compare word at ds:si to es:di, inc/dec si/di according to DF)

            instructionSet[0xa8] = null; // test al, imm8 (al AND imm8, set SF, ZF, PF, discard)
            instructionSet[0xa9] = null; // test ax, imm16 (ax AND imm16, set SF, ZF, PF, discard)

            instructionSet[0xaa] = null; // stosb (al -> es:di, inc/dec di according to DF)
            instructionSet[0xab] = null; // stosw (ax -> es:di, inc/dec di according to DF)
            instructionSet[0xac] = null; // lodsb (ds:si -> al, inc/dec si according to DF)
            instructionSet[0xad] = null; // lodsw (ds:si -> ax, inc/dec si according to DF)
            instructionSet[0xae] = null; // scasb (compare al <-> es:di)
            instructionSet[0xaf] = null; // scasw (compare ax <-> es:di)

            // mov reg,imm8
            instructionSet[0xb0] = null; // al
            instructionSet[0xb1] = null; // cl
            instructionSet[0xb2] = null; // dl
            instructionSet[0xb3] = null; // bl
            instructionSet[0xb4] = null; // ah
            instructionSet[0xb5] = null; // ch
            instructionSet[0xb6] = null; // dh
            instructionSet[0xb7] = null; // bh
            //mov reg,imm16
            instructionSet[0xb8] = null; // ax
            instructionSet[0xb9] = null; // cx
            instructionSet[0xba] = null; // dx
            instructionSet[0xbb] = null; // bx
            instructionSet[0xbc] = null; // sp
            instructionSet[0xbd] = null; // bp
            instructionSet[0xbe] = null; // si
            instructionSet[0xbf] = null; // di

            //nc
            instructionSet[0xc0] = null; //NC
            instructionSet[0xc1] = null; //NC

            //near returns
            instructionSet[0xc2] = null; // ret imm16 (return and pop imm16 bytes from stack)
            instructionSet[0xc3] = null; // ret (return to caller)

            //load segments
            instructionSet[0xc4] = null; // lds reg16/mem16 (load effective address into ds)
            instructionSet[0xc5] = null; // les reg16/mem16 (load effective address into es)

            instructionSet[0xc6] = null; // mov mem8, imm8
            instructionSet[0xc7] = null; // mov mem16, imm16

            //nc
            instructionSet[0xc8] = null; //NC
            instructionSet[0xc9] = null; //NC

            //far returns
            instructionSet[0xca] = null; // ret imm16 (far return and pop imm16 bytes)
            instructionSet[0xcb] = null; // ret (far return)

            //interrupts
            instructionSet[0xcc] = null; // int 3 (debugger trap)
            instructionSet[0xcd] = null; // int imm8
            instructionSet[0xce] = null; // into (overflow trap)
            instructionSet[0xcf] = null; // iret

            //rotate/shift (multiplexed with modrm bits 4-6)
            //000 - rol, 001 - ror, 010 - rcl, 011 - rcr, 100 - sal/shl, 101 - shr, 110 NC, 111 - SAR
            // 1-bit
            instructionSet[0xd0] = null; // reg8/mem8
            instructionSet[0xd1] = null; // reg16/mem16
            // cl-bits
            instructionSet[0xd2] = null; // reg8/mem8
            instructionSet[0xd3] = null; // reg16/mem16

            //ascii adjust
            instructionSet[0xd4] = null; // aam (byte 2 = 0x0a)
            instructionSet[0xd5] = null; // aad (byte 2 = 0x0a)

            instructionSet[0xd6] = null; //NC

            instructionSet[0xd7] = null; //xlat (al = [ds:bx])

            //esc instructions - emulating this is going to be.... interesting
            instructionSet[0xd8] = null;
            instructionSet[0xd9] = null;
            instructionSet[0xda] = null;
            instructionSet[0xdb] = null;
            instructionSet[0xdc] = null;
            instructionSet[0xdd] = null;
            instructionSet[0xde] = null;
            instructionSet[0xdf] = null;

            //short loops
            instructionSet[0xe0] = null; // loopne/loopnz inc8
            instructionSet[0xe1] = null; // loope/loopz inc8
            instructionSet[0xe2] = null; // loop inc8

            instructionSet[0xe3] = null; // jcxz inc8

            //in/out 8-bit port address
            instructionSet[0xe4] = null; // in byte [imm8]
            instructionSet[0xe5] = null; // in word [imm8]
            instructionSet[0xe6] = null; // out byte [imm8]
            instructionSet[0xe7] = null; // out word [imm8]

            //calls/jmps
            instructionSet[0xe8] = null; //call inc-lo, inc-hi (near)
            instructionSet[0xe9] = null; //jmp inc-lo, inc-hi (near)
            instructionSet[0xea] = null; //jmp ip-lo, ip-hi, cs-lo, cs-hi (far)
            instructionSet[0xeb] = null; //jmp inc8 (short)

            //in/out 16-bit port address
            instructionSet[0xec] = null; // in byte [dx]
            instructionSet[0xed] = null; // in word [dx]
            instructionSet[0xee] = null; //out byte [dx]
            instructionSet[0xef] = null; //out word [dx]

            instructionSet[0xf0] = null; // lock prefix

            instructionSet[0xf1] = null; //NC

            //repeat prefixes, expect CX to be seat
            instructionSet[0xf2] = null; //REPNE/REPNZ
            instructionSet[0xf3] = null; //REP/REPE/REPZ

            instructionSet[0xf4] = null; //HLT

            instructionSet[0xf5] = null; //CMC

            //multiplexed with modrm bits 4-6:
            //000 - test, 001 - NC, 010 - NOT, 011 - NEG, 100 - MUL, 101 - IMUL, 110 - DIV, 111 - IDIV
            instructionSet[0xf6] = null; //reg8/mem8, (imm8)
            instructionSet[0xf7] = null; //reg16/mem16, (imm16)

            instructionSet[0xf8] = null; //clc
            instructionSet[0xf9] = null; //stc
            instructionSet[0xfa] = null; //cli
            instructionSet[0xfb] = null; //sti
            instructionSet[0xfc] = null; //cld
            instructionSet[0xfd] = null; //std

            //multiplexed with modrm bits 4-6:
            //000 - inc, 001 - dec, 010/011/100/101/110/111 - NC
            instructionSet[0xfe] = null; // inc/dec reg8/mem8
            //000 - inc, 001 - dec, 010 - call (near indirect), 011 - call (far indirect), 100 - jmp (near indirect),
            //101 - jmp (far indirect), 110 - push mem16, 111 - nc
            instructionSet[0xff] = null;
        }

        #region mov_rm_reg
        // Moves data from:
        // Reg->Reg
        // Reg->ds:off
        // ds:off->reg
        private void mov_rm_reg()
        {
            if (mod == 3)
            {
                if (width)
                {
                    assign_register_register_word();
                }
                else
                {
                    assign_register_register_byte();
                }
                zeroize();
                return;
            }

            ushort displacement = get_displacement_from_rm();

            displacement += get_displacement_from_mod();

            if (direction)
            {
                if (width)
                {
                    assign_register_word(displacement);
                }
                else
                {
                    assign_register_byte(displacement);
                }
            }
            else
            {
                if (width)
                {
                    busInterfaceUnit.SetDataWord(displacement, registers[(WordGeneral)reg]);
                }
                else
                {
                    busInterfaceUnit.SetDataByte(displacement, registers[(ByteGeneral)reg]);
                }
            }

            zeroize();
        }
        #endregion

        #region mov_rm_immediate
        //moves data from immediate value to a register or memory
        //if mod==3, imm->reg
        //else, imm->mem
        private void mov_rm_immediate()
        {
            if (mod == 3)
            {
                if (width)
                {
                    fetch_data_word();
                    assign_register_word_immediate();
                }
                else
                {
                    fetch_data_byte();
                    assign_register_byte_immediate();
                }
                return;
            }

            ushort displacement = get_displacement_from_rm();
            displacement += get_displacement_from_mod();

            if (width)
            {
                fetch_data_word();
                busInterfaceUnit.SetDataWord(displacement, (ushort)((dataByteHigh << 8) | dataByteLow));
            }
            else
            {
                fetch_data_byte();
                busInterfaceUnit.SetDataByte(displacement, dataByteLow);
            }

            zeroize();
        }
        #endregion

        #region mov_reg_immediate
        //moves data from immediate value to a register
        private void mov_reg_immediate()
        {
            reg = (byte)(Opcode & 0b111);
            if ((opcode & 0b1000) != 0)
            {
                fetch_data_word();
                assign_register_word_immediate();
            }
            else
            {
                fetch_data_byte();
                assign_register_byte_immediate();
            }

            zeroize();
        }
        #endregion

        #region mov_acc_mem
        private void mov_acc_mem()
        {
            fetch_address_low();
            fetch_address_high();
            ushort displacement = (ushort)((addressHigh << 8) | addressLow);
            if (direction)
            {
                if (width)
                {
                    registers[WordGeneral.AX] = busInterfaceUnit.FetchDataWord(displacement);
                }
                else
                {
                    registers[ByteGeneral.AL] = busInterfaceUnit.FetchDataByte(displacement);
                }
            }
            else
            {
                if (width)
                {
                    busInterfaceUnit.SetDataWord(displacement, registers[WordGeneral.AX]);
                }
                else
                {
                    busInterfaceUnit.SetDataByte(displacement, registers[ByteGeneral.AL]);
                }
            }

            zeroize();
        }
        #endregion

        #region mov_seg_rm
        private void mov_seg_rm()
        {
            reg &= 0b00000011;
            if (mod == 3)
            {
                if (direction)
                {
                    registers[(Segment)reg] = registers[(WordGeneral)rm];
                }
                else
                {
                    registers[(WordGeneral)rm] = registers[(Segment)reg];
                }
                zeroize();
                return;
            }

            ushort displacement = get_displacement_from_rm();

            displacement += get_displacement_from_mod();

            if (direction)
            {
                registers[(Segment)reg] = busInterfaceUnit.FetchDataWord(displacement);
            }
            else
            {
                busInterfaceUnit.SetDataWord(displacement, registers[(Segment)reg]);
            }

            zeroize();
        }
        #endregion

        #region push_reg_mem
        private void push_reg_mem()
        {
            registers[WordGeneral.SP] -= 2;
            if (mod == 3)
            {
                busInterfaceUnit.Push(registers[(WordGeneral)rm]);
                zeroize();
                return;
            }

            ushort displacement = get_displacement_from_rm();
            displacement += get_displacement_from_mod();

            busInterfaceUnit.Push(busInterfaceUnit.FetchDataWord(displacement));

            zeroize();
        }
        #endregion

        #region push_reg
        private void push_reg()
        {
            reg = (byte)(Opcode & 0b111);
            registers.SP -= 2;
            busInterfaceUnit.Push(registers[(WordGeneral)reg]);

            zeroize();
        }
        #endregion

        #region push_pop_segment
        private void push_pop_segment()
        {
            byte segment = (byte)((Opcode & 0b00011000) >> 3);
            if (direction)
            {
                registers[(Segment)segment] = busInterfaceUnit.Pop();
                registers.SP += 2;
            }
            else
            {
                registers.SP -= 2;
                busInterfaceUnit.Push(registers[(Segment)segment]);
            }
            zeroize();
        }
        #endregion

        #region pop_reg_mem
        private void pop_reg_mem()
        {
            if (mod == 3)
            {
                registers[(WordGeneral)rm] = busInterfaceUnit.Pop();
                zeroize();
                registers.SP += 2;
                return;
            }

            ushort displacement = get_displacement_from_rm();
            displacement += get_displacement_from_mod();

            busInterfaceUnit.SetDataWord(displacement, busInterfaceUnit.Pop());
            zeroize();
            registers.SP += 2;
        }
        #endregion

        #region pop_reg
        private void pop_reg()
        {
            reg = (byte)(Opcode & 0b111);
            registers[(WordGeneral)reg] = busInterfaceUnit.Pop();
            registers.SP += 2;
            zeroize();
        }
        #endregion

        #region xchg_rm_reg
        private void xchg_rm_reg()
        {
            if (mod == 3)
            {
                if (width)
                {
                    tempWord = registers[(WordGeneral)reg];
                    registers[(WordGeneral)rm] = registers[(WordGeneral)reg];
                    registers[(WordGeneral)reg] = tempWord;
                }
                else
                {
                    tempByte = registers[(ByteGeneral)reg];
                    registers[(ByteGeneral)rm] = registers[(ByteGeneral)reg];
                    registers[(ByteGeneral)reg] = tempByte;
                }
                zeroize();
                return;
            }

            ushort displacement = get_displacement_from_rm();
            displacement += get_displacement_from_mod();

            if (width)
            {
                tempWord = busInterfaceUnit.FetchDataWord(displacement);
                busInterfaceUnit.SetDataWord(displacement, registers[(WordGeneral)reg]);
                registers[(WordGeneral)reg] = tempWord;
            }
            else
            {
                tempByte = busInterfaceUnit.FetchDataByte(displacement);
                busInterfaceUnit.SetDataByte(displacement, registers[(ByteGeneral)reg]);
                registers[(ByteGeneral)reg] = tempByte;
            }

            zeroize();
        }
        #endregion

        #region xcgh_reg_acc
        private void xchg_reg_acc()
        {
            byte reg = (byte)(Opcode & 0b111);
            tempWord = registers.AX;
            registers.AX = registers[(WordGeneral)reg];
            registers[(WordGeneral)reg] = tempWord;

            zeroize();
        }
        #endregion

        #region inout_imm8
        private void inout_imm8()
        {
            fetch_address_low();
            if (direction)
            {
                if (width)
                {
                    registers.AX = busInterfaceUnit.InWord(addressLow);
                }
                else
                {
                    registers.AL = busInterfaceUnit.InByte(addressLow);
                }
            }
            else
            {
                if (width)
                {
                    busInterfaceUnit.OutWord(addressLow, registers.AX);
                }
                else
                {
                    busInterfaceUnit.OutByte(addressLow, registers.AL);
                }
            }
            zeroize();
        }
        #endregion

        #region inout_dx
        private void inout_dx()
        {
            if (direction)
            {
                if (width)
                {
                    registers.AX = busInterfaceUnit.InWord(registers.DX);
                }
                else
                {
                    registers.AL = busInterfaceUnit.InByte(registers.DX);
                }
            }
            else
            {
                if (width)
                {
                    busInterfaceUnit.OutWord(registers.DX, registers.AX);
                }
                else
                {
                    busInterfaceUnit.OutByte(registers.DX, registers.AL);
                }
            }
            zeroize();
        }
        #endregion

        #region xlat
        private void xlat()
        {
            registers.AL = busInterfaceUnit.FetchDataByte((ushort)(registers.BX + registers.AL));
        }
        #endregion

        #region lea
        private void lea()
        {
            ushort displacement = get_effective_address();
            registers[(WordGeneral)reg] = displacement;

            zeroize();
        }
        #endregion
    }
}
