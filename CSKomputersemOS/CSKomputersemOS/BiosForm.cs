using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace CSKomputersemOS
{
    public class BiosForm : Form
    {
        private PictureBox logoPictureBox;
        private Label infoLabel;
        private Label fillerLabel;
        private RichTextBox consoleOutput;
        private Timer timer;
        private List<string> bootMessages;
        private int currentMessage = 0;
        private bool biosScreenShown = false;

        public BiosForm()
        {
            InitializeComponent();
            InitializeBootMessages();
            ShowBiosScreen();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(800, 600);
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

            logoPictureBox = new PictureBox
            {
                Size = new Size(442, 160),
                Location = new Point(300, 50),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            infoLabel = new Label
            {
                AutoSize = false,
                Size = new Size(600, 100),
                Location = new Point(100, 300),
                Text = "CSKomputersemOS BIOS v0.2\nCPU: Virtual 4-Core Processor\nRAM: - MB",
                Font = new Font("Consolas", 14),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            fillerLabel = new Label
            {
                AutoSize = false,
                Size = new Size(600, 100),
                Location = new Point(100, 450),
                Text = "Press F2 to enter setup\nPress F12 to enter boot menu",
                Font = new Font("Consolas", 12),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.BottomLeft
            };

            consoleOutput = new RichTextBox
            {
                Size = new Size(780, 580),
                Location = new Point(10, 10),
                BackColor = Color.Black,
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Visible = false
            };

            this.Controls.Add(logoPictureBox);
            this.Controls.Add(infoLabel);
            this.Controls.Add(fillerLabel);
            this.Controls.Add(consoleOutput);

            timer = new Timer
            {
                Interval = 30
            };
            timer.Tick += Timer_Tick;
        }

        private void InitializeBootMessages()
        {
            bootMessages = new List<string>
            {
                "[    0.000000] Linux version 5.10.0-CSKomputersemOS (root@cskomputersem) (gcc version 9.3.0, GNU ld version 2.34)",
                "[    0.000000] Command line: BOOT_IMAGE=/boot/vmlinuz-5.10.0-CSKomputersemOS root=UUID=1234-5678-90ab-cdef ro quiet splash vt.handoff=7",
                "[    0.000000] KERNEL supported cpus:",
                "[    0.000000]   Intel GenuineIntel",
                "[    0.000000]   AMD AuthenticAMD",
                "[    0.000000] x86/fpu: Supporting XSAVE feature 0x001: 'x87 floating point registers'",
                "[    0.000000] x86/fpu: Supporting XSAVE feature 0x002: 'SSE registers'",
                "[    0.000000] x86/fpu: Supporting XSAVE feature 0x004: 'AVX registers'",
                "[    0.000000] x86/fpu: xstate_offset[2]:  576, xstate_sizes[2]:  256",
                "[    0.000000] x86/fpu: Enabled xstate features 0x7, context size is 832 bytes, using 'standard' format.",
                "[    0.000000] BIOS-provided physical RAM map:",
                "[    0.000000] BIOS-e820: [mem 0x0000000000000000-0x000000000009fbff] usable",
                "[    0.000000] BIOS-e820: [mem 0x000000000009fc00-0x000000000009ffff] reserved",
                "[    0.000000] BIOS-e820: [mem 0x00000000000f0000-0x00000000000fffff] reserved",
                "[    0.000000] BIOS-e820: [mem 0x0000000000100000-0x00000000dffeffff] usable",
                "[    0.000000] BIOS-e820: [mem 0x00000000dfff0000-0x00000000dfffffff] ACPI data",
                "[    0.000000] BIOS-e820: [mem 0x00000000fec00000-0x00000000fec00fff] reserved",
                "[    0.000000] BIOS-e820: [mem 0x00000000fee00000-0x00000000fee00fff] reserved",
                "[    0.000000] BIOS-e820: [mem 0x00000000fffc0000-0x00000000ffffffff] reserved",
                "[    0.000000] NX (Execute Disable) protection: active",
                "[    0.000000] SMBIOS 2.7 present.",
                "[    0.000000] DMI: CSKomputersem Virtual Machine/CSKomputersem, BIOS 1.0 01/01/2021",
                "[    0.000000] Hypervisor detected: CSKomputersem",
                "[    0.000000] CSKomputersem Paravirt interface detected",
                "[    0.000000] tsc: Fast TSC calibration using PIT",
                "[    0.000000] tsc: Detected 3392.201 MHz processor",
                "[    0.000655] e820: update [mem 0x00000000-0x00000fff] usable ==> reserved",
                "[    0.000657] e820: remove [mem 0x000a0000-0x000fffff] usable",
                "[    0.000661] last_pfn = 0xdfff0 max_arch_pfn = 0x400000000",
                "[    0.000731] x86/PAT: Configuration [0-7]: WB  WC  UC- UC  WB  WP  UC- WT  ",
                "[    0.000958] found SMP MP-table at [mem 0x000f6a40-0x000f6a4f]",
                "[    0.001025] ACPI: Early table checksum verification disabled",
                "[    0.001028] ACPI: RSDP 0x00000000000F6900 000024 (v2 VBOX  )",
                "[    0.001030] ACPI: XSDT 0x00000000DFFF0030 00003C (v1 VBOX   VBOXXSDT 00000001 ASL  00000061)",
                "[    0.001033] ACPI: FACP 0x00000000DFFF00F0 0000F4 (v4 VBOX   VBOXFACP 00000001 ASL  00000061)",
                "[    0.001036] ACPI: DSDT 0x00000000DFFF0470 0021FF (v2 VBOX   VBOXBIOS 00000002 INTL 20200925)",
                "[    0.001038] ACPI: FACS 0x00000000DFFF0200 000040",
                "[    0.001040] ACPI: APIC 0x00000000DFFF0240 00005C (v2 VBOX   VBOXAPIC 00000001 ASL  00000061)",
                "[    0.001042] ACPI: SSDT 0x00000000DFFF02A0 0001CC (v1 VBOX   VBOXCPUT 00000002 INTL 20200925)",
                "[    0.001060] No NUMA configuration found",
                "[    0.001061] Faking a node at [mem 0x0000000000000000-0x00000000dffeffff]",
                "[    0.001064] NODE_DATA(0) allocated [mem 0xdffeb000-0xdffeffff]",
                "[    0.001134] Zone ranges:",
                "[    0.001135]   DMA      [mem 0x0000000000001000-0x0000000000ffffff]",
                "[    0.001136]   DMA32    [mem 0x0000000001000000-0x00000000dffeffff]",
                "[    0.001137]   Normal   empty",
                "[    0.001138] Movable zone start for each node",
                "[    0.001139] Early memory node ranges",
                "[    0.001139]   node   0: [mem 0x0000000000001000-0x000000000009efff]",
                "[    0.001140]   node   0: [mem 0x0000000000100000-0x00000000dffeffff]",
                "[    0.001141] Initmem setup node 0 [mem 0x0000000000001000-0x00000000dffeffff]",
                "[    0.001198] ACPI: PM-Timer IO Port: 0x4008",
                "[    0.001217] IOAPIC[0]: apic_id 2, version 32, address 0xfec00000, GSI 0-23",
                "[    0.001219] ACPI: INT_SRC_OVR (bus 0 bus_irq 0 global_irq 2 dfl dfl)",
                "[    0.001220] ACPI: INT_SRC_OVR (bus 0 bus_irq 9 global_irq 9 low level)",
                "[    0.001222] Using ACPI (MADT) for SMP configuration information",
                "[    0.001223] smpboot: Allowing 4 CPUs, 0 hotplug CPUs",
                "[    0.001235] PM: Registered nosave memory: [mem 0x00000000-0x00000fff]",
                "[    0.001236] PM: Registered nosave memory: [mem 0x0009f000-0x0009ffff]",
                "[    0.001237] PM: Registered nosave memory: [mem 0x000a0000-0x000effff]",
                "[    0.001237] PM: Registered nosave memory: [mem 0x000f0000-0x000fffff]",
                "[    0.001238] [mem 0xe0000000-0xfebfffff] available for PCI devices",
                "[    0.001239] Booting paravirtualized kernel on CSKomputersem",
                "[    0.001241] clocksource: refined-jiffies: mask: 0xffffffff max_cycles: 0xffffffff, max_idle_ns: 7645519600211568 ns",
                "[    0.001245] setup_percpu: NR_CPUS:8192 nr_cpumask_bits:4 nr_cpu_ids:4 nr_node_ids:1",
                "[    0.002384] percpu: Embedded 56 pages/cpu s192512 r8192 d28672 u524288",
                "[    0.002388] CSKomputersem setup: detected 4096MB RAM",
                "[    0.002389] CSKomputersem setup: reserving 128MB of RAM at 0xdc000000 for use as RAM",
                "[    0.002390] CSKomputersem setup: reserving 64MB of RAM at 0xdd000000 for use as RAM",
                "[    0.002391] CSKomputersem setup: reserving 64MB of RAM at 0xde000000 for use as RAM",
                "[    0.002435] Built 1 zonelists, mobility grouping on.  Total pages: 1032120",
                "[    0.002436] Policy zone: DMA32",
                "[    0.002437] Kernel command line: BOOT_IMAGE=/boot/vmlinuz-5.10.0-CSKomputersemOS root=UUID=1234-5678-90ab-cdef ro quiet splash vt.handoff=7",
                "[    0.003724] Dentry cache hash table entries: 524288 (order: 10, 4194304 bytes, linear)",
                "[    0.004024] Inode-cache hash table entries: 262144 (order: 9, 2097152 bytes, linear)",
                "[    0.004068] mem auto-init: stack:off, heap alloc:on, heap free:off",
                "[    0.033309] Memory: 3991756K/4193912K available (14339K kernel code, 2799K rwdata, 4960K rodata, 2628K init, 5056K bss, 202156K reserved, 0K cma-reserved)",
                "[    0.033313] random: get_random_u64 called from __kmem_cache_create+0x42/0x530 with crng_init=0",
                "[    0.034508] SLUB: HWalign=64, Order=0-3, MinObjects=0, CPUs=4, Nodes=1",
                "[    0.034517] Kernel/User page tables isolation: enabled",
                "[    0.035193] ftrace: allocating 44488 entries in 174 pages",
                "[    0.046961] rcu: Hierarchical RCU implementation.",
                "[    0.046962] rcu:     RCU restricting CPUs from NR_CPUS=8192 to nr_cpu_ids=4.",
                "[    0.046963] rcu: RCU calculated value of scheduler-enlistment delay is 25 jiffies.",
                "[    0.046964] rcu: Adjusting geometry for rcu_fanout_leaf=16, nr_cpu_ids=4",
                "[    0.048858] NR_IRQS: 524544, nr_irqs: 456, preallocated irqs: 16",
                "[    0.049378] Console: colour dummy device 80x25",
                "[    0.049380] printk: console [tty0] enabled",
                "[    0.049394] ACPI: Core revision 20200925",
                "[    0.049545] clocksource: hpet: mask: 0xffffffff max_cycles: 0xffffffff, max_idle_ns: 19112604467 ns",
                "[    0.049557] APIC: Switch to symmetric I/O mode setup",
                "[    0.049947] ..TIMER: vector=0x30 apic1=0 pin1=2 apic2=-1 pin2=-1",
                "[    0.050435] clocksource: tsc-early: mask: 0xffffffffffffffff max_cycles: 0x62c9c09081b, max_idle_ns: 440795235734 ns",
                "[    0.050438] Calibrating delay loop (skipped), value calculated using timer frequency.. 6784.40 BogoMIPS (lpj=13568804)",
                "[    0.050439] pid_max: default: 32768 minimum: 301",
                "[    0.051326] LSM: Security Framework initializing",
                "[    0.051335] Yama: becoming mindful.",
                "[    0.051375] AppArmor: AppArmor initialized",
                "[    0.051433] Mount-cache hash table entries: 8192 (order: 4, 65536 bytes, linear)",
                "[    0.051444] Mountpoint-cache hash table entries: 8192 (order: 4, 65536 bytes, linear)",
                "[    0.051453] *** VALIDATE proc ***",
                "[    0.051519] *** VALIDATE cgroup1 ***",
                "[    0.051520] *** VALIDATE cgroup2 ***",
                "[    0.051565] mce: CPU0: Thermal monitoring enabled (TM1)",
                "[    0.051578] process: using mwait in idle threads",
                "[    0.051579] Last level iTLB entries: 4KB 512, 2MB 8, 4MB 8",
                "[    0.051580] Last level dTLB entries: 4KB 512, 2MB 32, 4MB 32, 1GB 0",
                "[    0.051581] Spectre V1 : Mitigation: usercopy/swapgs barriers and __user pointer sanitization",
                "[    0.051582] Spectre V2 : Mitigation: Full generic retpoline",
                "[    0.051582] Spectre V2 : Spectre v2 / SpectreRSB mitigation: Filling RSB on context switch",
                "[    0.051583] Speculative Store Bypass: Mitigation: Speculative Store Bypass disabled via prctl and seccomp",
                "[    0.051584] MDS: Mitigation: Clear CPU buffers",
                "[    0.051681] Freeing SMP alternatives memory: 40K",
                "[    0.052755] smpboot: CPU0: CSKomputersem Virtual CPU (family: 0x6, model: 0x6a, stepping: 0x9)",
                "[    0.052844] Performance Events: PMU not available due to virtualization, using software events only.",
                "[    0.052868] rcu: Hierarchical SRCU implementation.",
                "[    0.053384] NMI watchdog: Perf NMI watchdog permanently disabled",
                "[    0.053391] smp: Bringing up secondary CPUs ...",
                "[    0.053441] x86: Booting SMP configuration:",
                "[    0.053442] .... node  #0, CPUs:      #1",
                "[    0.053935] CSKomputersem reported: CPU #1 initialized",
                "[    0.054439]  #2",
                            };
        }

        private async void ShowBiosScreen()
        {
            string logoPath = Path.Combine(Application.StartupPath, "UI", "bios_logo.png");
            if (File.Exists(logoPath))
            {
                logoPictureBox.Image = Image.FromFile(logoPath);
            }
            else
            {
                Console.WriteLine("Logo file not found: " + logoPath);
            }

            await Task.Delay(3000);  // Показываем экран BIOS на 3 секунды
            logoPictureBox.Visible = false;
            infoLabel.Visible = false;
            fillerLabel.Visible = false;
            consoleOutput.Visible = true;
            biosScreenShown = true;
            StartBootSequence();
        }

        private async void StartBootSequence()
        {
            await Task.Delay(1000);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (biosScreenShown && currentMessage < bootMessages.Count)
            {
                for (int i = 0; i < 5 && currentMessage < bootMessages.Count; i++)
                {
                    consoleOutput.AppendText(bootMessages[currentMessage] + "\n");
                    currentMessage++;
                }
                consoleOutput.ScrollToCaret();
            }
            else if (currentMessage >= bootMessages.Count)
            {
                timer.Stop();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}