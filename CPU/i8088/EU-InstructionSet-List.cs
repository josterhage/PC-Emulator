using System;

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

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
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
                        or_r8_rm8,     // 0x0a or reg8 reg8/mem8
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
                        esc_d8,         // 0xd8
                        esc_d9,         // 0xd9
                        esc_da,         // 0xda
                        esc_db,         // 0xdb
                        esc_dc,         // 0xdc
                        esc_dd,         // 0xdd
                        esc_de,         // 0xde
                        esc_df,         // 0xdf

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
        }
    }
}


