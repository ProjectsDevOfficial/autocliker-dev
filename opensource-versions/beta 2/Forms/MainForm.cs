using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using AutoClicker.Models;
using AutoClicker.Services;

namespace AutoClicker.Forms
{
    public partial class MainForm : Form
    {
        private ClickSettings _settings = new ClickSettings();
        private HotkeyService? _hotkeyService;
        private System.Windows.Forms.Timer _clickTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer _statusTimer = new System.Windows.Forms.Timer();
        private int _currentClickCount = 0;
        private bool _isClicking = false;
        private NotifyIcon _notifyIcon = new NotifyIcon();
        private bool _isRecording = false;
        private List<ClickSequence> _recordedSequence = new List<ClickSequence>();

        public MainForm()
        {
            InitializeComponent();
            InitializeTimer();
            InitializeNotifyIcon();
            InitializeGlobalHotkeys();
            LoadSettings();
            UpdateUI();
        }

        private void InitializeComponent()
        {
            this.Text = "AutoClikerDev";
            this.Size = new Size(650, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(650, 750);
            try
            {
                this.Icon = new Icon("icon.ico");
            }
            catch
            {
                this.Icon = SystemIcons.Application;
            }

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var title = new Label
            {
                Text = "AutoClikerDev",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(0, 120, 215)
            };

            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var basicTab = CreateBasicTab();
            basicTab.Text = "Основные настройки";
            var advancedTab = CreateAdvancedTab();
            advancedTab.Text = "Расширенные настройки";
            var sequencesTab = CreateSequencesTab();
            sequencesTab.Text = "Последовательности кликов";
            var hotkeysTab = CreateHotkeysTab();
            hotkeysTab.Text = "Горячие клавиши";

            tabControl.TabPages.Add(basicTab);
            tabControl.TabPages.Add(advancedTab);
            tabControl.TabPages.Add(sequencesTab);
            tabControl.TabPages.Add(hotkeysTab);

            var statusPanel = CreateStatusPanel();

            mainPanel.Controls.Add(statusPanel);
            mainPanel.Controls.Add(tabControl);
            mainPanel.Controls.Add(title);

            this.Controls.Add(mainPanel);
        }

        private TabPage CreateBasicTab()
        {
            var tab = new TabPage("Основные настройки");
            tab.Padding = new Padding(10);

            var y = 10;

            // Интервал кликов
            tab.Controls.Add(new Label { Text = "Интервал кликов:", Location = new Point(10, y), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) });
            
            y += 25;
            var hoursLabel = new Label { Text = "Часы:", Location = new Point(20, y), AutoSize = true };
            var hoursTextBox = new TextBox { Location = new Point(70, y - 3), Width = 50, Text = "0" };
            tab.Controls.Add(hoursLabel);
            tab.Controls.Add(hoursTextBox);

            var minutesLabel = new Label { Text = "Минуты:", Location = new Point(130, y), AutoSize = true };
            var minutesTextBox = new TextBox { Location = new Point(190, y - 3), Width = 50, Text = "0" };
            tab.Controls.Add(minutesLabel);
            tab.Controls.Add(minutesTextBox);

            var secondsLabel = new Label { Text = "Секунды:", Location = new Point(250, y), AutoSize = true };
            var secondsTextBox = new TextBox { Location = new Point(310, y - 3), Width = 50, Text = "0" };
            tab.Controls.Add(secondsLabel);
            tab.Controls.Add(secondsTextBox);

            var millisecondsLabel = new Label { Text = "Миллисекунды:", Location = new Point(370, y), AutoSize = true };
            var millisecondsTextBox = new TextBox { Location = new Point(470, y - 3), Width = 80, Text = _settings.ClickInterval.ToString() };
            millisecondsTextBox.TextChanged += (s, e) => { if (int.TryParse(millisecondsTextBox.Text, out var val)) _settings.ClickInterval = val; };
            tab.Controls.Add(millisecondsLabel);
            tab.Controls.Add(millisecondsTextBox);

            y += 35;
            tab.Controls.Add(new Label { Text = "Случайное смещение +-:", Location = new Point(10, y), AutoSize = true });
            var randomOffsetTextBox = new TextBox { Location = new Point(150, y - 3), Width = 80, Text = "40" };
            tab.Controls.Add(randomOffsetTextBox);
            tab.Controls.Add(new Label { Text = "миллисекунд", Location = new Point(240, y), AutoSize = true });

            y += 35;
            var separator1 = new Label { Text = "─────────────────────────────────────", Location = new Point(10, y), AutoSize = true, ForeColor = Color.Gray };
            tab.Controls.Add(separator1);

            y += 25;
            tab.Controls.Add(new Label { Text = "Опции клика", Location = new Point(10, y), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) });

            y += 25;
            tab.Controls.Add(new Label { Text = "Кнопка мыши:", Location = new Point(20, y), AutoSize = true });
            var clickTypeCombo = new ComboBox { Location = new Point(120, y - 3), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            clickTypeCombo.Items.AddRange(new[] { "Левая", "Правая", "Средняя" });
            clickTypeCombo.SelectedIndex = 0;
            clickTypeCombo.SelectedIndexChanged += (s, e) => 
            {
                _settings.ClickType = clickTypeCombo.SelectedIndex switch
                {
                    0 => ClickType.Left,
                    1 => ClickType.Right,
                    2 => ClickType.Middle,
                    _ => ClickType.Left
                };
            };
            tab.Controls.Add(clickTypeCombo);

            y += 35;
            tab.Controls.Add(new Label { Text = "Тип клика:", Location = new Point(20, y), AutoSize = true });
            var clickModeCombo = new ComboBox { Location = new Point(120, y - 3), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            clickModeCombo.Items.AddRange(new[] { "Одиночный", "Двойной", "Тройной" });
            clickModeCombo.SelectedIndex = 0;
            clickModeCombo.SelectedIndexChanged += (s, e) => 
            {
                _settings.ClickMode = clickModeCombo.SelectedIndex switch
                {
                    0 => ClickMode.Single,
                    1 => ClickMode.Double,
                    2 => ClickMode.Triple,
                    _ => ClickMode.Single
                };
            };
            tab.Controls.Add(clickModeCombo);

            y += 35;
            var separator2 = new Label { Text = "─────────────────────────────────────", Location = new Point(10, y), AutoSize = true, ForeColor = Color.Gray };
            tab.Controls.Add(separator2);

            y += 25;
            tab.Controls.Add(new Label { Text = "Повтор кликов", Location = new Point(10, y), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) });

            y += 25;
            tab.Controls.Add(new Label { Text = "Повторить", Location = new Point(20, y), AutoSize = true });
            var repeatTextBox = new TextBox { Location = new Point(90, y - 3), Width = 60, Text = _settings.ClickCount.ToString() };
            repeatTextBox.TextChanged += (s, e) => { if (int.TryParse(repeatTextBox.Text, out var val)) _settings.ClickCount = val; };
            tab.Controls.Add(repeatTextBox);
            tab.Controls.Add(new Label { Text = "раз", Location = new Point(160, y), AutoSize = true });

            y += 35;
            var infiniteCheckBox = new CheckBox { Text = "Повторять до остановки", Location = new Point(20, y), Checked = _settings.InfiniteClicks, Width = 200 };
            infiniteCheckBox.CheckedChanged += (s, e) => { _settings.InfiniteClicks = infiniteCheckBox.Checked; repeatTextBox.Enabled = !infiniteCheckBox.Checked; };
            tab.Controls.Add(infiniteCheckBox);

            y += 35;
            var separator3 = new Label { Text = "─────────────────────────────────────", Location = new Point(10, y), AutoSize = true, ForeColor = Color.Gray };
            tab.Controls.Add(separator3);

            y += 25;
            tab.Controls.Add(new Label { Text = "Позиция курсора", Location = new Point(10, y), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) });

            y += 25;
            var currentPositionRadio = new RadioButton { Text = "Текущее положение", Location = new Point(20, y), Checked = _settings.UseCurrentPosition, Width = 150 };
            currentPositionRadio.CheckedChanged += (s, e) => { _settings.UseCurrentPosition = currentPositionRadio.Checked; };
            tab.Controls.Add(currentPositionRadio);

            y += 35;
            var fixedPositionRadio = new RadioButton { Text = "Выбрать положение", Location = new Point(20, y), Checked = !_settings.UseCurrentPosition, Width = 150 };
            fixedPositionRadio.CheckedChanged += (s, e) => { _settings.UseCurrentPosition = !fixedPositionRadio.Checked; };
            tab.Controls.Add(fixedPositionRadio);

            y += 35;
            tab.Controls.Add(new Label { Text = "X:", Location = new Point(40, y), AutoSize = true });
            var xPosTextBox = new TextBox { Location = new Point(60, y - 3), Width = 80, Text = _settings.ClickPosition.X.ToString() };
            xPosTextBox.TextChanged += (s, e) => { if (int.TryParse(xPosTextBox.Text, out var val)) { var pos = _settings.ClickPosition; pos.X = val; _settings.ClickPosition = pos; } };
            tab.Controls.Add(xPosTextBox);

            tab.Controls.Add(new Label { Text = "Y:", Location = new Point(150, y), AutoSize = true });
            var yPosTextBox = new TextBox { Location = new Point(170, y - 3), Width = 80, Text = _settings.ClickPosition.Y.ToString() };
            yPosTextBox.TextChanged += (s, e) => { if (int.TryParse(yPosTextBox.Text, out var val)) { var pos = _settings.ClickPosition; pos.Y = val; _settings.ClickPosition = pos; } };
            tab.Controls.Add(yPosTextBox);

            var getPositionButton = new Button { Text = "Получить позицию", Location = new Point(260, y - 3), Width = 120, Height = 25 };
            getPositionButton.Click += (s, e) => { var pos = MouseService.GetCursorPosition(); xPosTextBox.Text = pos.X.ToString(); yPosTextBox.Text = pos.Y.ToString(); };
            tab.Controls.Add(getPositionButton);

            return tab;
        }

        private TabPage CreateAdvancedTab()
        {
            var tab = new TabPage("Расширенные настройки");
            tab.Padding = new Padding(10);

            var y = 10;

            var randomizationCheckBox = new CheckBox { Text = "Включить рандомизацию", Location = new Point(10, y), Checked = _settings.EnableRandomization, Width = 200 };
            randomizationCheckBox.CheckedChanged += (s, e) => { _settings.EnableRandomization = randomizationCheckBox.Checked; };
            tab.Controls.Add(randomizationCheckBox);

            y += 35;
            tab.Controls.Add(new Label { Text = "Случайная задержка мин (мс):", Location = new Point(10, y), AutoSize = true });
            var randomMinTextBox = new TextBox { Location = new Point(200, y - 3), Width = 100, Text = _settings.RandomDelayMin.ToString() };
            randomMinTextBox.TextChanged += (s, e) => { if (int.TryParse(randomMinTextBox.Text, out var val)) _settings.RandomDelayMin = val; };
            tab.Controls.Add(randomMinTextBox);

            y += 35;
            tab.Controls.Add(new Label { Text = "Случайная задержка макс (мс):", Location = new Point(10, y), AutoSize = true });
            var randomMaxTextBox = new TextBox { Location = new Point(200, y - 3), Width = 100, Text = _settings.RandomDelayMax.ToString() };
            randomMaxTextBox.TextChanged += (s, e) => { if (int.TryParse(randomMaxTextBox.Text, out var val)) _settings.RandomDelayMax = val; };
            tab.Controls.Add(randomMaxTextBox);

            y += 35;
            tab.Controls.Add(new Label { Text = "Случайный диапазон X:", Location = new Point(10, y), AutoSize = true });
            var randomXTextBox = new TextBox { Location = new Point(200, y - 3), Width = 100, Text = _settings.RandomXRange.ToString() };
            randomXTextBox.TextChanged += (s, e) => { if (int.TryParse(randomXTextBox.Text, out var val)) _settings.RandomXRange = val; };
            tab.Controls.Add(randomXTextBox);

            y += 35;
            tab.Controls.Add(new Label { Text = "Случайный диапазон Y:", Location = new Point(10, y), AutoSize = true });
            var randomYTextBox = new TextBox { Location = new Point(200, y - 3), Width = 100, Text = _settings.RandomYRange.ToString() };
            randomYTextBox.TextChanged += (s, e) => { if (int.TryParse(randomYTextBox.Text, out var val)) _settings.RandomYRange = val; };
            tab.Controls.Add(randomYTextBox);

            y += 35;
            var humanSimulationCheckBox = new CheckBox { Text = "Имитация человеческого поведения", Location = new Point(10, y), Checked = _settings.EnableHumanSimulation, Width = 250 };
            humanSimulationCheckBox.CheckedChanged += (s, e) => { _settings.EnableHumanSimulation = humanSimulationCheckBox.Checked; };
            tab.Controls.Add(humanSimulationCheckBox);

            y += 35;
            tab.Controls.Add(new Label { Text = "Вариация скорости (0.0-1.0):", Location = new Point(10, y), AutoSize = true });
            var speedVariationTextBox = new TextBox { Location = new Point(200, y - 3), Width = 100, Text = _settings.HumanSpeedVariation.ToString() };
            speedVariationTextBox.TextChanged += (s, e) => { if (double.TryParse(speedVariationTextBox.Text, out var val)) _settings.HumanSpeedVariation = Math.Max(0, Math.Min(1, val)); };
            tab.Controls.Add(speedVariationTextBox);

            y += 35;
            var soundCheckBox = new CheckBox { Text = "Включить звуковые эффекты", Location = new Point(10, y), Checked = _settings.EnableSound, Width = 200 };
            soundCheckBox.CheckedChanged += (s, e) => { _settings.EnableSound = soundCheckBox.Checked; };
            tab.Controls.Add(soundCheckBox);

            y += 35;
            var minimizeCheckBox = new CheckBox { Text = "Сворачивать в трей", Location = new Point(10, y), Checked = _settings.MinimizeToTray, Width = 200 };
            minimizeCheckBox.CheckedChanged += (s, e) => { _settings.MinimizeToTray = minimizeCheckBox.Checked; };
            tab.Controls.Add(minimizeCheckBox);

            return tab;
        }

        private TabPage CreateSequencesTab()
        {
            var tab = new TabPage("Последовательности кликов");
            tab.Padding = new Padding(10);

            var sequenceList = new ListBox { Location = new Point(10, 10), Size = new Size(400, 200) };
            var addButton = new Button { Text = "Добавить клик", Location = new Point(420, 10), Width = 120, Height = 30 };
            var removeButton = new Button { Text = "Удалить", Location = new Point(420, 50), Width = 120, Height = 30 };
            var clearButton = new Button { Text = "Очистить всё", Location = new Point(420, 90), Width = 120, Height = 30 };

            tab.Controls.Add(sequenceList);
            tab.Controls.Add(addButton);
            tab.Controls.Add(removeButton);
            tab.Controls.Add(clearButton);

            var y = 220;
            tab.Controls.Add(new Label { Text = "Запись и воспроизведение:", Location = new Point(10, y), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) });

            y += 30;
            var recordButton = new Button { Text = "Начать запись", Location = new Point(10, y), Width = 120, Height = 30, BackColor = Color.Red, ForeColor = Color.White };
            var playButton = new Button { Text = "Воспроизвести", Location = new Point(140, y), Width = 120, Height = 30, BackColor = Color.Green, ForeColor = Color.White };
            var stopRecordButton = new Button { Text = "Остановить запись", Location = new Point(270, y), Width = 140, Height = 30, BackColor = Color.Orange, ForeColor = Color.White, Enabled = false };

            tab.Controls.Add(recordButton);
            tab.Controls.Add(playButton);
            tab.Controls.Add(stopRecordButton);

            y += 40;
            tab.Controls.Add(new Label { Text = "Повторить последовательность:", Location = new Point(10, y), AutoSize = true });
            var repeatTextBox = new TextBox { Location = new Point(200, y - 3), Width = 100, Text = _settings.RepeatSequences.ToString() };
            repeatTextBox.TextChanged += (s, e) => { if (int.TryParse(repeatTextBox.Text, out var val)) _settings.RepeatSequences = val; };
            tab.Controls.Add(repeatTextBox);

            addButton.Click += (s, e) => 
            {
                var pos = MouseService.GetCursorPosition();
                var sequence = new ClickSequence { Position = pos, ClickType = _settings.ClickType };
                _settings.Sequences.Add(sequence);
                sequenceList.Items.Add($"Клик в ({pos.X}, {pos.Y}) - {GetClickTypeName(sequence.ClickType)}");
            };

            removeButton.Click += (s, e) => 
            {
                if (sequenceList.SelectedIndex >= 0)
                {
                    _settings.Sequences.RemoveAt(sequenceList.SelectedIndex);
                    sequenceList.Items.RemoveAt(sequenceList.SelectedIndex);
                }
            };

            clearButton.Click += (s, e) => 
            {
                _settings.Sequences.Clear();
                sequenceList.Items.Clear();
            };

            recordButton.Click += (s, e) => 
            {
                _isRecording = true;
                _recordedSequence.Clear();
                recordButton.Enabled = false;
                stopRecordButton.Enabled = true;
                recordButton.Text = "Запись...";
            };

            stopRecordButton.Click += (s, e) => 
            {
                _isRecording = false;
                recordButton.Enabled = true;
                stopRecordButton.Enabled = false;
                recordButton.Text = "Начать запись";
                
                _settings.Sequences.Clear();
                sequenceList.Items.Clear();
                
                foreach (var seq in _recordedSequence)
                {
                    _settings.Sequences.Add(seq);
                    sequenceList.Items.Add($"Клик в ({seq.Position.X}, {seq.Position.Y}) - {GetClickTypeName(seq.ClickType)}");
                }
            };

            playButton.Click += async (s, e) => 
            {
                if (_settings.Sequences.Count == 0) return;
                
                var sequenceService = new ClickSequenceService(_settings);
                await sequenceService.StartSequenceAsync();
            };

            return tab;
        }

        private TabPage CreateHotkeysTab()
        {
            var tab = new TabPage("Горячие клавиши");
            tab.Padding = new Padding(10);

            var y = 10;

            var enableHotkeysCheckBox = new CheckBox { Text = "Включить горячие клавиши", Location = new Point(10, y), Checked = _settings.EnableHotkeys, Width = 200 };
            enableHotkeysCheckBox.CheckedChanged += (s, e) => { _settings.EnableHotkeys = enableHotkeysCheckBox.Checked; };
            tab.Controls.Add(enableHotkeysCheckBox);

            y += 35;
            tab.Controls.Add(new Label { Text = "Клавиша запуска:", Location = new Point(10, y), AutoSize = true });
            var startHotkeyTextBox = new TextBox { Location = new Point(150, y - 3), Width = 100, Text = _settings.StartHotkey };
            startHotkeyTextBox.TextChanged += (s, e) => { _settings.StartHotkey = startHotkeyTextBox.Text; };
            tab.Controls.Add(startHotkeyTextBox);

            y += 35;
            tab.Controls.Add(new Label { Text = "Клавиша остановки:", Location = new Point(10, y), AutoSize = true });
            var stopHotkeyTextBox = new TextBox { Location = new Point(150, y - 3), Width = 100, Text = _settings.StopHotkey };
            stopHotkeyTextBox.TextChanged += (s, e) => { _settings.StopHotkey = stopHotkeyTextBox.Text; };
            tab.Controls.Add(stopHotkeyTextBox);

            y += 50;
            tab.Controls.Add(new Label { Text = "Доступные клавиши:", Location = new Point(10, y), Font = new Font("Arial", 10, FontStyle.Bold) });

            y += 25;
            var keysLabel = new Label 
            { 
                Text = "F1-F12, Пробел, Enter, Esc, Tab, Delete, Home, End,\nPageUp, PageDown, Insert, Стрелки, NumLock,\nScrollLock, CapsLock, Pause, PrintScreen,\nИли комбинировать с Ctrl, Alt, Shift, Win",
                Location = new Point(10, y),
                AutoSize = true
            };
            tab.Controls.Add(keysLabel);

            return tab;
        }

        private Panel CreateStatusPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 140,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var startButton = new Button
            {
                Text = "Старт (F6)",
                Location = new Point(10, 10),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(0, 200, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            startButton.Click += StartClicking;

            var stopButton = new Button
            {
                Text = "Стоп (F6)",
                Location = new Point(140, 10),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(200, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Enabled = false
            };
            stopButton.Click += StopClicking;

            var settingsButton = new Button
            {
                Text = "Настройки хоткеев",
                Location = new Point(270, 10),
                Size = new Size(150, 40),
                FlatStyle = FlatStyle.Flat
            };
            settingsButton.Click += (s, e) => { /* Открыть настройки хоткеев */ };

            var recordButton = new Button
            {
                Text = "Запись и воспроизведение",
                Location = new Point(430, 10),
                Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat
            };
            recordButton.Click += (s, e) => { /* Открыть запись */ };

            var statusLabel = new Label
            {
                Text = "Статус: Готов",
                Location = new Point(10, 60),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            var clickCountLabel = new Label
            {
                Text = "Клики: 0",
                Location = new Point(200, 60),
                AutoSize = true,
                Font = new Font("Arial", 10)
            };

            var positionLabel = new Label
            {
                Text = "Позиция: (0, 0)",
                Location = new Point(350, 60),
                AutoSize = true,
                Font = new Font("Arial", 10)
            };

            var timeLabel = new Label
            {
                Text = "Время: 00:00:00",
                Location = new Point(10, 85),
                AutoSize = true,
                Font = new Font("Arial", 10)
            };

            var githubButton = new Button
            {
                Text = "Исходный код",
                Location = new Point(620, 10),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.White
            };
            githubButton.Click += (s, e) => 
            {
                try
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo 
                    { 
                        FileName = "https://github.com/ProjectsDevOfficial/autocliker-dev", 
                        UseShellExecute = true 
                    });
                }
                catch { }
            };

            panel.Controls.AddRange(new Control[] { startButton, stopButton, settingsButton, recordButton, githubButton, statusLabel, clickCountLabel, positionLabel, timeLabel });

            startButton.Tag = new Tuple<Label, Label, Label, Label>(statusLabel, clickCountLabel, positionLabel, timeLabel);
            stopButton.Tag = new Tuple<Label, Label, Label, Label>(statusLabel, clickCountLabel, positionLabel, timeLabel);

            return panel;
        }

        private string GetClickTypeName(ClickType clickType)
        {
            return clickType switch
            {
                ClickType.Left => "Левая",
                ClickType.Right => "Правая",
                ClickType.Middle => "Средняя",
                _ => "Неизвестно"
            };
        }

        private void InitializeGlobalHotkeys()
        {
            if (_settings.EnableHotkeys)
            {
                _hotkeyService = new HotkeyService(this.Handle);
                _hotkeyService.RegisterHotkey(_settings.StartHotkey, () => 
                {
                    if (!_isClicking)
                        StartClicking(null, EventArgs.Empty);
                });
                _hotkeyService.RegisterHotkey(_settings.StopHotkey, () => StopClicking(null, EventArgs.Empty));
            }
        }

        private void InitializeTimer()
        {
            _clickTimer.Tick += PerformClick;
            _statusTimer.Interval = 100;
            _statusTimer.Tick += UpdateStatus;
            _statusTimer.Start();
        }

        private void InitializeNotifyIcon()
        {
            try
            {
                _notifyIcon.Icon = new Icon("icon.ico");
            }
            catch
            {
                _notifyIcon.Icon = SystemIcons.Application;
            }
            _notifyIcon.Text = "AutoClikerDev";
            _notifyIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Показать", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; });
            contextMenu.Items.Add("Старт", null, (s, e) => StartClicking(null, EventArgs.Empty));
            contextMenu.Items.Add("Стоп", null, (s, e) => StopClicking(null, EventArgs.Empty));
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Исходный код", null, (s, e) => 
            {
                try
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo 
                    { 
                        FileName = "https://github.com/ProjectsDevOfficial/autocliker-dev", 
                        UseShellExecute = true 
                    });
                }
                catch { }
            });
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Выход", null, (s, e) => Application.Exit());

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void StartClicking(object? sender, EventArgs e)
        {
            if (_isClicking) return;

            _isClicking = true;
            _currentClickCount = 0;

            // Регистрируем хоткей для остановки
            if (_settings.EnableHotkeys)
            {
                if (_hotkeyService == null)
                {
                    _hotkeyService = new HotkeyService(this.Handle);
                }
                _hotkeyService.RegisterHotkey(_settings.StopHotkey, () => StopClicking(null, EventArgs.Empty));
            }

            _clickTimer.Interval = GetClickInterval();
            _clickTimer.Start();

            if (sender is Button button)
            {
                button.Enabled = false;
                var stopButton = this.Controls.Find("stopButton", true).FirstOrDefault() as Button;
                if (stopButton != null) stopButton.Enabled = true;

                var tags = button.Tag as Tuple<Label, Label, Label, Label>;
                if (tags?.Item1 != null) tags.Item1.Text = "Статус: Кликает...";
            }

            if (_settings.EnableSound)
                SystemSounds.Beep.Play();
        }

        private void StopClicking(object? sender, EventArgs e)
        {
            if (!_isClicking) return;

            _isClicking = false;
            _clickTimer.Stop();

            _hotkeyService?.Dispose();
            _hotkeyService = null;

            if (sender is Button button || sender == null)
            {
                var startButton = this.Controls.Find("startButton", true).FirstOrDefault() as Button;
                var stopButton = this.Controls.Find("stopButton", true).FirstOrDefault() as Button;
                
                if (startButton != null) startButton.Enabled = true;
                if (stopButton != null) stopButton.Enabled = false;

                var tags = startButton?.Tag as Tuple<Label, Label, Label, Label>;
                if (tags?.Item1 != null) tags.Item1.Text = "Статус: Остановлен";
            }

            if (_settings.EnableSound)
                SystemSounds.Hand.Play();
        }

        private void PerformClick(object? sender, EventArgs e)
        {
            if (!_isClicking) return;

            Point clickPosition;
            if (_settings.UseCurrentPosition)
            {
                clickPosition = MouseService.GetCursorPosition();
            }
            else
            {
                clickPosition = _settings.ClickPosition;
            }

            if (_settings.EnableRandomization)
            {
                clickPosition = MouseService.GetRandomizedPosition(clickPosition, _settings.RandomXRange, _settings.RandomYRange);
            }

            switch (_settings.ClickMode)
            {
                case ClickMode.Single:
                    MouseService.Click(_settings.ClickType, clickPosition);
                    break;
                case ClickMode.Double:
                    MouseService.DoubleClick(_settings.ClickType, clickPosition);
                    break;
                case ClickMode.Triple:
                    MouseService.TripleClick(_settings.ClickType, clickPosition);
                    break;
            }

            _currentClickCount++;

            if (!_settings.InfiniteClicks && _currentClickCount >= _settings.ClickCount)
            {
                StopClicking(null, EventArgs.Empty);
            }

            _clickTimer.Interval = GetClickInterval();
        }

        private int GetClickInterval()
        {
            int interval = _settings.ClickInterval;

            if (_settings.EnableRandomization)
            {
                var random = new Random();
                interval = random.Next(_settings.RandomDelayMin, _settings.RandomDelayMax + 1);
            }

            if (_settings.EnableHumanSimulation)
            {
                var random = new Random();
                var variation = (int)(interval * _settings.HumanSpeedVariation);
                interval += random.Next(-variation, variation + 1);
            }

            return Math.Max(1, interval);
        }

        private void UpdateStatus(object? sender, EventArgs e)
        {
            var panel = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Bottom);
            if (panel == null) return;

            var clickCountLabel = panel.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Клики:"));
            if (clickCountLabel != null)
                clickCountLabel.Text = $"Клики: {_currentClickCount}";

            var positionLabel = panel.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Позиция:"));
            if (positionLabel != null)
            {
                var pos = MouseService.GetCursorPosition();
                positionLabel.Text = $"Позиция: ({pos.X}, {pos.Y})";
            }

            var timeLabel = panel.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Время:"));
            if (timeLabel != null && _isClicking)
            {
                var elapsed = TimeSpan.FromMilliseconds(_currentClickCount * _settings.ClickInterval);
                timeLabel.Text = $"Время: {elapsed:hh\\:mm\\:ss}";
            }
        }

        private void SaveSettings(object? sender, EventArgs e)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(_settings, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText("settings.json", json);
                MessageBox.Show("Настройки успешно сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (System.IO.File.Exists("settings.json"))
                {
                    var json = System.IO.File.ReadAllText("settings.json");
                    _settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ClickSettings>(json) ?? new ClickSettings();
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSettings(object? sender, EventArgs e)
        {
            LoadSettings();
        }

        private void UpdateUI()
        {
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (_hotkeyService != null && id == 1)
                {
                    StopClicking(null, EventArgs.Empty);
                }
            }
            base.WndProc(ref m);
        }

        protected override void OnResize(EventArgs e)
        {
            if (_settings.MinimizeToTray && WindowState == FormWindowState.Minimized)
            {
                Hide();
                _notifyIcon.Visible = true;
                _notifyIcon.ShowBalloonTip(1000, "AutoClikerDev", "Сворачиваю в трей", ToolTipIcon.Info);
            }
            
            // Адаптация интерфейса при изменении размера
            AdjustControlsLayout();
            
            base.OnResize(e);
        }

        private void AdjustControlsLayout()
        {
            // Можно добавить логику для адаптации элементов при изменении размера
            // Например, изменение размера кнопок в статусной панели
            var statusPanel = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Bottom);
            if (statusPanel != null && this.Width > 800)
            {
                // Расширяем кнопки при большом окне
                foreach (var button in statusPanel.Controls.OfType<Button>())
                {
                    button.Width = Math.Max(button.Width, 140);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                _notifyIcon.Visible = true;
                return;
            }

            _hotkeyService?.Dispose();
            _notifyIcon?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
