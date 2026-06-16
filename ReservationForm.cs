using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Waiting_Reservation.classs;

namespace Waiting_Reservation
{
    public class ReservationForm : Form
    {
        private readonly RestaurantType  _type;
        private readonly Color           _accent, _accentLight;
        private readonly string          _name;
        private readonly ReservationMenu _menu;

        private int      _step         = 1;
        private DateTime _selectedDate = DateTime.MinValue;
        private string   _selectedTime = "";
        private int      _partySize    = 2;

        private readonly List<Panel>  _datePanels = new();
        private readonly List<Button> _timeBtns   = new();
        private Label _lblParty = null!;

        private TextBox _tbName = null!, _tbPhone = null!, _tbRequest = null!;
        private Label   _lblSumm = null!;

        private Panel _pnlReceipt = null!;
        private Label _lblRecNum  = null!;
        private string _headerSubText = "날짜를 선택해 주세요";

        private Panel  _pnlStepper = null!, _pnlHeader  = null!;
        private Panel  _pnlFooter  = null!, _pnlContent = null!;
        private Panel  _pnlStep1   = null!, _pnlStep2   = null!;
        private Panel  _pnlStep3   = null!, _pnlStep4   = null!;
        private Button _btnPrev    = null!, _btnNext     = null!;

        private const int STEPPER_H = 62;
        private const int HEADER_H = 112;
        private const int FOOTER_H  = 68;
        private int _cw, _ch, _contentY, _contentH;

        private static readonly string[] TIME_SLOTS = {
            "11:00","11:30","12:00","12:30","13:00","13:30",
            "17:00","17:30","18:00","18:30","19:00","19:30","20:00"
        };
        private static readonly string[] DAY_KR = { "일","월","화","수","목","금","토" };

        public ReservationForm(RestaurantType type, ReservationMenu menu)
        {
            _type        = type;
            _accent      = UIHelper.GetAccent(type);
            _accentLight = UIHelper.GetLightBg(type);
            _name        = UIHelper.GetName(type);
            _menu        = menu;

            Text            = $"{_name} 예약";
            BackColor       = UIHelper.BgColor;
            Size            = new Size(920, 860);   // 높이 20px 증가
            MinimumSize     = new Size(920, 860);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            Font            = new Font(UIHelper.FONT, 9.5f);

            _cw       = ClientSize.Width;
            _ch       = ClientSize.Height;
            _contentY = STEPPER_H + HEADER_H;          // 158
            // Footer 를 contentH 계산에서 제외: Step4(완료)에서 footer가 숨겨져 공간 확보
            _contentH = _ch - _contentY;               // ~644

            BuildStepper();
            BuildHeader();
            BuildFooter();
            BuildContent();
            GoToStep(1);
        }

        // ══════════════════════════════════════════
        //  구조 패널 (모두 절대좌표)
        // ══════════════════════════════════════════
        private void BuildStepper()
        {
            _pnlStepper = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(_cw, STEPPER_H),
                BackColor = Color.White,
            };
            _pnlStepper.Paint += DrawStepper;
            Controls.Add(_pnlStepper);
        }

        private void BuildHeader()
        {
            _pnlHeader = new Panel
            {
                Location  = new Point(0, STEPPER_H),
                Size      = new Size(_cw, HEADER_H),
                BackColor = _accent,
            };
            _pnlHeader.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var br = new LinearGradientBrush(_pnlHeader.ClientRectangle,
                    _accent, ControlPaint.Dark(_accent, 0.15f), LinearGradientMode.Horizontal);
                g.FillRectangle(br, _pnlHeader.ClientRectangle);
                using var tf = new Font(UIHelper.FONT, 17f, FontStyle.Bold);
                g.DrawString($"{UIHelper.GetEmoji(_type)}  {_name} 예약", tf, Brushes.White, 110f, 26f);
                using var sf = new Font(UIHelper.FONT, 9.5f);
                g.DrawString(_headerSubText, sf, Brushes.White, 114f, 72f);
            };

            var btnBack = UIHelper.Btn("↩", Color.Transparent, Color.White, 14f);
            btnBack.Location = new Point(14, 26); btnBack.Size = new Size(80, 30);
            btnBack.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, Color.White);
            btnBack.Click   += (s, e) => { _menu.Show(); Close(); };
            _pnlHeader.Controls.Add(btnBack);

            Controls.Add(_pnlHeader);
        }

        private void BuildFooter()
        {
            _pnlFooter = new Panel
            {
                Location  = new Point(0, _ch - FOOTER_H),
                Size      = new Size(_cw, FOOTER_H),
                BackColor = Color.White,
            };

            _btnPrev = UIHelper.Btn("← 이전", UIHelper.BorderColor, UIHelper.TextPrimary);
            _btnPrev.Size = new Size(140, 46); _btnPrev.Location = new Point(18, 11);
            UIHelper.SetRounded(_btnPrev, 8);
            _btnPrev.Click += (s, e) => GoToStep(_step - 1);
            _pnlFooter.Controls.Add(_btnPrev);

            _btnNext = UIHelper.Btn("다음 단계 →", _accent);
            _btnNext.Size = new Size(_cw - 174, 46); _btnNext.Location = new Point(170, 11);
            UIHelper.SetRounded(_btnNext, 8);
            _btnNext.Click += (s, e) => HandleNext();
            _pnlFooter.Controls.Add(_btnNext);

            Controls.Add(_pnlFooter);
        }

        private void BuildContent()
        {
            _pnlContent = new Panel
            {
                Location   = new Point(0, _contentY),
                Size       = new Size(_cw, _contentH),
                BackColor  = UIHelper.BgColor,
                AutoScroll = false,
            };
            Controls.Add(_pnlContent);

            // 4개 Step 패널 — 같은 위치, Visible로 전환
            int sw = _cw - 56;
            int sh = _contentH - 20;

            _pnlStep1 = MakeStep(sw, sh);
            _pnlStep2 = MakeStep(sw, sh);
            _pnlStep3 = MakeStep(sw, sh);
            _pnlStep4 = MakeStep(sw, sh);

            _pnlContent.Controls.Add(_pnlStep1);
            _pnlContent.Controls.Add(_pnlStep2);
            _pnlContent.Controls.Add(_pnlStep3);
            _pnlContent.Controls.Add(_pnlStep4);

            BuildStep1(sw, sh);
            BuildStep2(sw, sh);
            BuildStep3(sw, sh);
            BuildStep4(sw, sh);
        }

        private Panel MakeStep(int w, int h) =>
            new Panel { Location = new Point(28, 10), Size = new Size(w, h),
                        BackColor = UIHelper.BgColor, Visible = false };

        // ── STEP 1 : 날짜
        private void BuildStep1(int sw, int sh)
        {
            _pnlStep1.Controls.Add(UIHelper.Lbl("방문 날짜를 선택하세요", 13, FontStyle.Bold));

            var sub = UIHelper.Lbl("오늘부터 2주 이내 날짜를 선택할 수 있습니다",
                9.5f, FontStyle.Regular, UIHelper.TextMuted);
            sub.Location = new Point(0, 42);
            _pnlStep1.Controls.Add(sub);

            var flow = new FlowLayoutPanel
            {
                Location      = new Point(0, 82),
                Size          = new Size(sw, 104),
                WrapContents  = false,
                BackColor     = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
            };
            _pnlStep1.Controls.Add(flow);

            for (int i = 0; i < 14; i++)
            {
                var date   = DateTime.Today.AddDays(i);
                bool isSun = date.DayOfWeek == DayOfWeek.Sunday;
                bool isSat = date.DayOfWeek == DayOfWeek.Saturday;

                var card = new Panel
                {
                    Size      = new Size(66, 94),
                    Margin    = new Padding(0, 0, 10, 0),
                    BackColor = UIHelper.BgColor,
                    Cursor    = Cursors.Hand,
                    Tag       = date,
                };

                var lblDay = new Label { Text=DAY_KR[(int)date.DayOfWeek], AutoSize=false, Size=new Size(66,20), Location=new Point(0,8), TextAlign=ContentAlignment.MiddleCenter, Font=new Font(UIHelper.FONT,8f), BackColor=Color.Transparent, ForeColor=isSun?UIHelper.Danger:isSat?Color.FromArgb(44,82,140):UIHelper.TextMuted };
                var lblNum = new Label { Text=date.Day.ToString(), AutoSize=false, Size=new Size(66,36), Location=new Point(0,26), TextAlign=ContentAlignment.MiddleCenter, Font=new Font(UIHelper.FONT,16f,FontStyle.Bold), BackColor=Color.Transparent, ForeColor=UIHelper.TextPrimary };
                var lblTod = new Label { Text=i==0?"오늘":"", AutoSize=false, Size=new Size(66,16), Location=new Point(0,64), TextAlign=ContentAlignment.MiddleCenter, Font=new Font(UIHelper.FONT,7.5f,FontStyle.Bold), BackColor=Color.Transparent, ForeColor=_accent };
                card.Controls.AddRange(new Control[]{ lblDay, lblNum, lblTod });

                card.Paint += (s, e) =>
                {
                    bool sel = _selectedDate == (DateTime)((Panel)s).Tag;
                    UIHelper.PaintCard(e.Graphics,
                        new Rectangle(0, 0, ((Panel)s).Width, ((Panel)s).Height),
                        12, sel ? _accent : Color.White,
                        sel ? _accent : UIHelper.BorderColor);
                };
                EventHandler onClick = (s, e) =>
                {
                    Control src = (Control)s;
                    Panel target = src is Panel p2 ? p2 : (Panel)src.Parent!;
                    _selectedDate = (DateTime)target.Tag;
                    RefreshDateCards();
                };
                card.Click += onClick;
                foreach (Control c in card.Controls) c.Click += onClick;
                _datePanels.Add(card);
                flow.Controls.Add(card);
            }
        }

        private void RefreshDateCards()
        {
            foreach (var card in _datePanels)
            {
                bool sel   = _selectedDate == (DateTime)card.Tag;
                var  date  = (DateTime)card.Tag;
                bool isSun = date.DayOfWeek == DayOfWeek.Sunday;
                bool isSat = date.DayOfWeek == DayOfWeek.Saturday;
                card.Invalidate();
                foreach (Control c in card.Controls)
                {
                    if (c is not Label lbl) continue;
                    if (lbl.Font.Size >= 14)    lbl.ForeColor = sel ? Color.White : UIHelper.TextPrimary;
                    else if (lbl.Text == "오늘") lbl.ForeColor = sel ? Color.White : _accent;
                    else                        lbl.ForeColor = sel ? Color.White
                        : isSun ? UIHelper.Danger : isSat ? Color.FromArgb(44,82,140) : UIHelper.TextMuted;
                }
            }
        }

        // ── STEP 2 : 시간 + 인원
        private void BuildStep2(int sw, int sh)
        {
            _pnlStep2.Controls.Add(UIHelper.Lbl("예약 시간을 선택하세요", 13, FontStyle.Bold));

            var flow = new FlowLayoutPanel
            {
                Location = new Point(0, 48), Size = new Size(sw, 120), BackColor = UIHelper.BgColor
            };
            foreach (var slot in TIME_SLOTS)
            {
                var btn = UIHelper.Btn(slot, Color.White, UIHelper.TextPrimary, 10.5f);
                btn.Size = new Size(106, 44); btn.Margin = new Padding(0, 0, 8, 8);
                btn.FlatAppearance.BorderSize  = 1;
                btn.FlatAppearance.BorderColor = UIHelper.BorderColor;
                btn.Tag   = slot;
                btn.Click += OnTimeSelected;
                _timeBtns.Add(btn);
                flow.Controls.Add(btn);
            }
            _pnlStep2.Controls.Add(flow);

            var lblP = UIHelper.Lbl("방문 인원", 13, FontStyle.Bold);
            lblP.Location = new Point(0, 186); _pnlStep2.Controls.Add(lblP);

            var row = new Panel { Location=new Point(0,208), Size=new Size(280,54), BackColor=Color.Transparent };
            var bm  = UIHelper.Btn("－", UIHelper.BorderColor, UIHelper.TextPrimary, 12f);
            bm.Size=new Size(50,50); bm.Location=new Point(0,0); UIHelper.SetRounded(bm,8);
            bm.Click += (s,e)=>{ if(_partySize>1){_partySize--;_lblParty.Text=$"{_partySize}명";}};
            row.Controls.Add(bm);
            _lblParty = new Label { Text="2명", Size=new Size(110,50), Location=new Point(58,0),
                TextAlign=ContentAlignment.MiddleCenter, Font=new Font(UIHelper.FONT,17f,FontStyle.Bold),
                ForeColor=UIHelper.TextPrimary, BackColor=Color.Transparent };
            row.Controls.Add(_lblParty);
            var bp = UIHelper.Btn("＋", _accent, Color.White, 12f);
            bp.Size=new Size(50,50); bp.Location=new Point(176,0); UIHelper.SetRounded(bp,8);
            bp.Click += (s,e)=>{ if(_partySize<20){_partySize++;_lblParty.Text=$"{_partySize}명";}};
            row.Controls.Add(bp);
            _pnlStep2.Controls.Add(row);
        }

        private void OnTimeSelected(object? sender, EventArgs e)
        {
            _selectedTime = (string)((Button)sender!).Tag!;
            foreach (var b in _timeBtns)
            {
                bool on = b.Tag?.ToString() == _selectedTime;
                b.BackColor = on ? _accent : Color.White;
                b.ForeColor = on ? Color.White : UIHelper.TextPrimary;
                b.FlatAppearance.BorderColor = on ? _accent : UIHelper.BorderColor;
            }
        }

        // ── STEP 3 : 정보 입력
        private void BuildStep3(int sw, int sh)
        {
            _pnlStep3.Controls.Add(UIHelper.Lbl("예약자 정보를 입력하세요", 13, FontStyle.Bold));

            var summCard = UIHelper.CardPanel(0, 50, sw, 52, 12);
            _lblSumm = new Label { AutoSize=false, Size=new Size(sw-32,52), Location=new Point(16,0),
                TextAlign=ContentAlignment.MiddleLeft, Font=new Font(UIHelper.FONT,10f),
                ForeColor=UIHelper.TextPrimary, BackColor=Color.Transparent };
            summCard.Controls.Add(_lblSumm);
            _pnlStep3.Controls.Add(summCard);

            int y = 122;
            AddField(_pnlStep3, sw, "예약자 이름 *", ref y, out _tbName,    "이름을 입력하세요");
            AddField(_pnlStep3, sw, "연락처 *",       ref y, out _tbPhone,   "010-0000-0000");

            var reqLbl = UIHelper.Lbl("요청사항 (선택)", 9.5f, FontStyle.Bold, UIHelper.TextMuted);
            reqLbl.Location = new Point(0, y); _pnlStep3.Controls.Add(reqLbl); y += 26;

            _tbRequest = new TextBox { Location=new Point(0,y), Size=new Size(sw,86), Multiline=true,
                PlaceholderText="알레르기, 좌석 배치 등 요청사항을 입력하세요",
                BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle,
                Font=new Font(UIHelper.FONT,10.5f) };
            _pnlStep3.Controls.Add(_tbRequest);
        }

        private void AddField(Panel parent, int sw, string label, ref int y,
            out TextBox tb, string placeholder)
        {
            var lbl0 = UIHelper.Lbl(label, 9.5f, FontStyle.Bold, UIHelper.TextMuted);
            lbl0.Location = new Point(0, y);
            parent.Controls.Add(lbl0);
            y += 26;
            tb = UIHelper.TB(placeholder);
            tb.Location = new Point(0, y); tb.Size = new Size(sw, 40);
            parent.Controls.Add(tb); y += 56;
        }

        // ── STEP 4 : 완료
        private void BuildStep4(int sw, int sh)
        {
            int cardW = Math.Min(540, sw);
            int cardX = (sw - cardW) / 2;
            // 높이를 sh에 맞게 조정 (최대 460 또는 가용 높이)
            int cardH = Math.Min(460, sh - 20);
            var card  = UIHelper.CardPanel(cardX, 8, cardW, cardH, 16);
            _pnlStep4.Controls.Add(card);

            // 체크 원
            var iconP = new Panel { Size=new Size(60,60), Location=new Point((cardW-60)/2, 22),
                BackColor=_accentLight };
            iconP.Paint += (s, e) =>
            {
                UIHelper.PaintCard(e.Graphics, new Rectangle(0,0,60,60), 30, _accentLight, _accent, 2f);
                using var f = new Font("Segoe UI Emoji", 20f);
                using var b = new SolidBrush(_accent);
                var sz = e.Graphics.MeasureString("✓", f);
                e.Graphics.DrawString("✓", f, b, (60-sz.Width)/2f, (60-sz.Height)/2f);
            };
            card.Controls.Add(iconP);

            var lblDone = new Label { Text="예약이 완료되었습니다!", AutoSize=false,
                Size=new Size(cardW-32,34), Location=new Point(16,92),
                TextAlign=ContentAlignment.MiddleCenter,
                Font=new Font(UIHelper.FONT,14f,FontStyle.Bold),
                ForeColor=UIHelper.TextPrimary, BackColor=Color.Transparent };
            card.Controls.Add(lblDone);

            _lblRecNum = new Label { AutoSize=false, Size=new Size(cardW-32,22),
                Location=new Point(16,128), TextAlign=ContentAlignment.MiddleCenter,
                Font=new Font(UIHelper.FONT,9.5f), ForeColor=UIHelper.TextMuted, BackColor=Color.Transparent };
            card.Controls.Add(_lblRecNum);

            var sep = new Panel { Location=new Point(24,160), Size=new Size(cardW-48,1),
                BackColor=UIHelper.BorderColor };
            card.Controls.Add(sep);

            _pnlReceipt = new Panel { Location=new Point(24,170),
                Size=new Size(cardW-48, cardH-170-56), BackColor=Color.Transparent,
                AutoScroll=true };
            card.Controls.Add(_pnlReceipt);

            var btnClose = UIHelper.Btn("레스토랑 목록으로 돌아가기", _accent);
            btnClose.Size     = new Size(cardW-48, 48);
            btnClose.Location = new Point(24, cardH-58);
            UIHelper.SetRounded(btnClose, 10);
            btnClose.Click   += (s, e) => { _menu.Show(); Close(); };
            card.Controls.Add(btnClose);
        }

        // ══════════════════════════════════════════
        //  스텝퍼 그리기
        // ══════════════════════════════════════════
        private void DrawStepper(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            string[] labels = { "날짜","시간·인원","정보입력","완료" };
            int cx0=80, cx3=_cw-80, span=cx3-cx0, cy=24, dotR=12;

            for (int i = 0; i < 4; i++)
            {
                int cx = cx0 + (i * span / 3);
                if (i < 3)
                {
                    int nx = cx0 + ((i+1)*span/3);
                    using var pen = new Pen(_step>i+1?_accent:UIHelper.BorderColor, 2f);
                    g.DrawLine(pen, cx+dotR, cy, nx-dotR, cy);
                }
                var dc = _step>=i+1 ? _accent : Color.FromArgb(210,206,200);
                using (var path = UIHelper.RoundedPath(new Rectangle(cx-dotR,cy-dotR,dotR*2,dotR*2),dotR))
                using (var br   = new SolidBrush(dc)) g.FillPath(br, path);

                string num = _step>i+1?"✓":(i+1).ToString();
                using var nf = new Font(UIHelper.FONT,7.5f,FontStyle.Bold);
                var ns = g.MeasureString(num,nf);
                g.DrawString(num,nf,Brushes.White,cx-ns.Width/2f,cy-ns.Height/2f);

                bool active = _step==i+1;
                using var lf = new Font(UIHelper.FONT,8f,active?FontStyle.Bold:FontStyle.Regular);
                using var lb = new SolidBrush(active?_accent:UIHelper.TextMuted);
                var ls = g.MeasureString(labels[i],lf);
                g.DrawString(labels[i],lf,lb,cx-ls.Width/2f,cy+dotR+4);
            }
        }

        // ══════════════════════════════════════════
        //  네비게이션
        // ══════════════════════════════════════════
        private void GoToStep(int step)
        {
            _step = step;
            _pnlStep1.Visible = step==1; _pnlStep2.Visible = step==2;
            _pnlStep3.Visible = step==3; _pnlStep4.Visible = step==4;
            _pnlFooter.Visible = step < 4;
            _btnPrev.Visible   = step > 1;
            _btnNext.Text      = step==3 ? "예약 완료하기  ✓" : "다음 단계  →";

            // 헤더 부제목 업데이트 (Paint 에서 직접 그림)
            _headerSubText = step switch
            {
                1 => "날짜를 선택해 주세요",
                2 => "시간과 인원을 선택해 주세요",
                3 => "예약자 정보를 입력해 주세요",
                4 => "예약이 완료되었습니다!",
                _ => ""
            };
            _pnlHeader?.Invalidate();

            if (step==3 && _lblSumm!=null)
                _lblSumm.Text =
                    $"📅  {_selectedDate:MM월 dd일 (ddd)}     🕐  {_selectedTime}     👥  {_partySize}명";

            _pnlStepper.Invalidate();
        }

        private void HandleNext()
        {
            if (_step==1)
            {
                if (_selectedDate==DateTime.MinValue)
                { MessageBox.Show("날짜를 선택해주세요."); return; }
                GoToStep(2);
            }
            else if (_step==2)
            {
                if (_selectedTime=="")
                { MessageBox.Show("시간을 선택해주세요."); return; }
                GoToStep(3);
            }
            else if (_step==3)
            {
                if (string.IsNullOrWhiteSpace(_tbName.Text))
                { MessageBox.Show("이름을 입력해주세요."); return; }
                if (string.IsNullOrWhiteSpace(_tbPhone.Text))
                { MessageBox.Show("전화번호를 입력해주세요."); return; }

                var item = new ReservationItem
                {
                    Restaurant=_type, RestaurantName=_name,
                    Name=_tbName.Text.Trim(), Phone=_tbPhone.Text.Trim(),
                    Date=_selectedDate, Time=_selectedTime,
                    PartySize=_partySize, Request=_tbRequest.Text.Trim(),
                };
                ReservationStore.Add(item);
                BuildReceipt(item);
                GoToStep(4);
            }
        }

        private void BuildReceipt(ReservationItem item)
        {
            _pnlReceipt.Controls.Clear();
            _lblRecNum.Text = $"예약번호  #{item.Id:D4}";

            var rows = new List<(string k, string v)>
            {
                ("식당",   item.RestaurantName),
                ("날짜",   item.Date.ToString("yyyy년 MM월 dd일 (ddd)")),
                ("시간",   item.Time),
                ("인원",   $"{item.PartySize}명"),
                ("예약자", item.Name),
                ("연락처", item.Phone),
            };
            if (!string.IsNullOrWhiteSpace(item.Request))
                rows.Add(("요청사항", item.Request));

            int y = 0;
            int valW = _pnlReceipt.Width - 80;

            foreach (var (k, v) in rows)
            {
                // 값 라벨: AutoSize + MaximumSize 로 긴 텍스트 자동 줄바꿈
                var valLbl = new Label
                {
                    Text        = v,
                    AutoSize    = true,
                    MaximumSize = new Size(valW, 0),   // 가로 고정, 세로 자동
                    MinimumSize = new Size(valW, 28),
                    Location    = new Point(80, y),
                    ForeColor   = UIHelper.TextPrimary,
                    Font        = new Font(UIHelper.FONT, 9.5f, FontStyle.Bold),
                    BackColor   = Color.Transparent,
                };

                // 키 라벨 높이 = 값 라벨 높이에 맞춤
                var keyLbl = new Label
                {
                    Text      = k,
                    AutoSize  = false,
                    Size      = new Size(72, Math.Max(valLbl.Height, 28)),
                    Location  = new Point(0, y),
                    TextAlign = ContentAlignment.TopLeft,
                    ForeColor = UIHelper.TextMuted,
                    Font      = new Font(UIHelper.FONT, 9f),
                    BackColor = Color.Transparent,
                };

                _pnlReceipt.Controls.Add(keyLbl);
                _pnlReceipt.Controls.Add(valLbl);
                y += Math.Max(valLbl.Height, 28) + 8;
            }
        }
    }
}
