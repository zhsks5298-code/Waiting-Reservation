using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Waiting_Reservation.classs;

namespace Waiting_Reservation
{
    public static class MyWaiting
    {
        public static ListViewItem?  MyItem       = null;
        public static RestaurantType MyRestaurant = RestaurantType.Outback;
    }

    public class WaitingForm : Form
    {
        private readonly RestaurantType _type;
        private readonly Color          _accent;
        private readonly WaitingMenu    _menu;

        private readonly System.Windows.Forms.Timer _timer = new();

        private ListView _lv        = null!;
        private TextBox  _tbName    = null!;
        private TextBox  _tbPhone   = null!;
        private Label    _lblParty  = null!;
        private int      _partySize = 2;

        private Label  _lblMyStatus = null!;
        private Button _btnCancelMy = null!;

        private const int HEADER_H = 106;

        public WaitingForm(RestaurantType type, WaitingMenu menu)
        {
            _type   = type;
            _accent = UIHelper.GetAccent(type);
            _menu   = menu;

            Text            = $"{UIHelper.GetName(type)} - 웨이팅";
            BackColor       = UIHelper.BgColor;
            Size            = new Size(1020, 860);
            MinimumSize     = new Size(1020, 860);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;

            // Shown 이벤트에서 레이아웃 확정 (ClientSize가 정확히 확정된 시점)
            Shown += (s, e) => BuildLayout();

            // 컨트롤 미리 생성 (레이아웃 전)
            CreateControls();
        }

        // ── 컨트롤 생성 (위치/크기는 BuildLayout에서 결정)
        private void CreateControls()
        {
            // 헤더
            var header = new Panel { Location = new Point(0,0), BackColor = _accent };
            header.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var br = new LinearGradientBrush(header.ClientRectangle,
                    _accent, ControlPaint.Dark(_accent, 0.15f), LinearGradientMode.Horizontal);
                g.FillRectangle(br, header.ClientRectangle);
                // 텍스트를 Paint 에서 직접 그림 (투명 Label 렌더링 깨짐 방지)
                using var tf = new Font(UIHelper.FONT, 17f, FontStyle.Bold);
                g.DrawString($"{UIHelper.GetEmoji(_type)}  {UIHelper.GetName(_type)}",
                    tf, Brushes.White, 108f, 24f);
                using var sf = new Font(UIHelper.FONT, 9.5f);
                g.DrawString(UIHelper.GetCategory(_type), sf, Brushes.White, 112f, 68f);
            };
            var btnBack = UIHelper.Btn("↩", Color.Transparent, Color.White, 14f);
            btnBack.Location = new Point(14, 24); btnBack.Size = new Size(80, 30);
            btnBack.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, Color.White);
            btnBack.Click += (s, e) => { _menu.Show(); Hide(); };
            header.Controls.Add(btnBack);

            Controls.Add(header);
            header.Name = "pnlHeader";
        }

        // ── 레이아웃 확정 (Shown 이후 호출)
        private void BuildLayout()
        {
            int cw = ClientSize.Width;
            int ch = ClientSize.Height;

            // 헤더 크기 확정
            var header = (Panel)Controls["pnlHeader"]!;
            header.Size = new Size(cw, HEADER_H);

            int contentTop = HEADER_H + 14;          // 헤더 아래 14px 여유
            int contentH   = ch - contentTop - 14;   // 하단 14px 여유
            int pad        = 16;
            int gap        = 14;
            int leftW      = (int)(cw * 0.56);
            int rightW     = cw - leftW - pad * 3 - gap;

            // 왼쪽 패널
            var leftPanel = new Panel
            {
                Location  = new Point(pad, contentTop),
                Size      = new Size(leftW, contentH),
                BackColor = UIHelper.BgColor,
            };
            Controls.Add(leftPanel);
            BuildLeftPanel(leftPanel, leftW, contentH);

            // 오른쪽 패널
            var rightPanel = new Panel
            {
                Location  = new Point(pad + leftW + gap, contentTop),
                Size      = new Size(rightW, contentH),
                BackColor = UIHelper.BgColor,
            };
            Controls.Add(rightPanel);
            BuildRightPanel(rightPanel, rightW, contentH);

            // 컨트롤을 앞으로 가져오기 (헤더가 최상위)
            header.BringToFront();

            LoadInitialData();
            StartTimer();
        }

        // ══════════════════════════════════════════
        //  왼쪽: 대기 현황 + ListView
        // ══════════════════════════════════════════
        private void BuildLeftPanel(Panel p, int w, int h)
        {
            var lblTitle = UIHelper.Lbl("대기 현황", 13, FontStyle.Bold);
            lblTitle.Location = new Point(0, 0);
            p.Controls.Add(lblTitle);

            // 일반 ListView (OwnerDraw 없음 — 렌더링 안정성 우선)
            _lv = new ListView
            {
                Location      = new Point(0, 36),
                Size          = new Size(w, h - 36),
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = false,
                BackColor     = Color.White,
                Font          = new Font(UIHelper.FONT, 10f),
                BorderStyle   = BorderStyle.None,
                HeaderStyle   = ColumnHeaderStyle.Nonclickable,
            };

            _lv.Columns.Add("순번",   56);
            _lv.Columns.Add("이름",   100);
            _lv.Columns.Add("연락처", 148);
            _lv.Columns.Add("인원",   60);
            _lv.Columns.Add("상태",   w - 56 - 100 - 148 - 60 - 20);

            p.Controls.Add(_lv);
        }

        // ══════════════════════════════════════════
        //  오른쪽: 등록 카드 + 내 대기
        // ══════════════════════════════════════════
        private void BuildRightPanel(Panel p, int w, int h)
        {
            // ── 등록 카드
            int cardH = Math.Min(440, (int)(h * 0.58));
            var card  = UIHelper.CardPanel(0, 0, w, cardH, 12);
            p.Controls.Add(card);

            var lblReg = UIHelper.Lbl("대기 등록", 13, FontStyle.Bold);
            lblReg.Location = new Point(16, 14);
            card.Controls.Add(lblReg);

            int y = 50;
            AddField(card, w, "이름",   ref y, out _tbName,  "이름을 입력하세요");
            AddField(card, w, "연락처", ref y, out _tbPhone, "010-0000-0000");

            var lblPL = UIHelper.Lbl("인원", 9.5f, FontStyle.Bold, UIHelper.TextMuted);
            lblPL.Location = new Point(16, y); card.Controls.Add(lblPL); y += 26;

            var row = new Panel { Location=new Point(16,y), Size=new Size(w-32,52), BackColor=Color.Transparent };
            var bm  = UIHelper.Btn("－", UIHelper.BorderColor, UIHelper.TextPrimary, 12f);
            bm.Size=new Size(48,48); bm.Location=new Point(0,0); UIHelper.SetRounded(bm,8);
            bm.Click += (s,e)=>{if(_partySize>1){_partySize--;_lblParty.Text=$"{_partySize}명";}};
            row.Controls.Add(bm);
            _lblParty = new Label { Text="2명", Size=new Size(100,48), Location=new Point(56,0),
                TextAlign=ContentAlignment.MiddleCenter, Font=new Font(UIHelper.FONT,16f,FontStyle.Bold),
                ForeColor=UIHelper.TextPrimary, BackColor=Color.Transparent };
            row.Controls.Add(_lblParty);
            var bp = UIHelper.Btn("＋", _accent, Color.White, 12f);
            bp.Size=new Size(48,48); bp.Location=new Point(164,0); UIHelper.SetRounded(bp,8);
            bp.Click += (s,e)=>{if(_partySize<20){_partySize++;_lblParty.Text=$"{_partySize}명";}};
            row.Controls.Add(bp);
            card.Controls.Add(row); y += 62;

            var btnAdd = UIHelper.Btn("대기 등록하기", _accent, Color.White, 11f);
            btnAdd.Location=new Point(16,y); btnAdd.Size=new Size(w-32,48);
            UIHelper.SetRounded(btnAdd, 10);
            btnAdd.Click += OnAddClicked;
            card.Controls.Add(btnAdd); y += 58;

            var btnDel = UIHelper.Btn("선택 항목 삭제", UIHelper.BorderColor, UIHelper.TextPrimary, 9.5f);
            btnDel.Location=new Point(16,y); btnDel.Size=new Size(w-32,38);
            UIHelper.SetRounded(btnDel, 8);
            btnDel.Click += OnDeleteClicked;
            card.Controls.Add(btnDel);

            // ── 내 대기 현황 카드
            int myY = cardH + 12;
            int myH = h - myY;
            if (myH > 100)
            {
                var myCard = UIHelper.CardPanel(0, myY, w, myH, 12);
                p.Controls.Add(myCard);

                var lblT = UIHelper.Lbl("내 대기 현황", 12, FontStyle.Bold);
                lblT.Location = new Point(16, 14); myCard.Controls.Add(lblT);

                _lblMyStatus = new Label
                {
                    AutoSize=false, Size=new Size(w-32, 80), Location=new Point(16,46),
                    Font=new Font(UIHelper.FONT, 10f), ForeColor=UIHelper.TextPrimary,
                    BackColor=Color.Transparent,
                };
                myCard.Controls.Add(_lblMyStatus);

                _btnCancelMy = UIHelper.Btn("대기 취소", UIHelper.Danger);
                _btnCancelMy.Location = new Point(16, myH - 58);
                _btnCancelMy.Size     = new Size(w-32, 40);
                UIHelper.SetRounded(_btnCancelMy, 8);
                _btnCancelMy.Click += (s,e)=>
                {
                    if (MyWaiting.MyItem!=null && MyWaiting.MyRestaurant==_type)
                    {
                        _lv.Items.Remove(MyWaiting.MyItem);
                        MyWaiting.MyItem = null;
                        _menu.UpdateWaitingCount(_type, _lv.Items.Count);
                        RefreshMyStatus();
                    }
                };
                myCard.Controls.Add(_btnCancelMy);
            }
            RefreshMyStatus();
        }

        private void AddField(Panel parent, int w, string label, ref int y, out TextBox tb, string ph)
        {
            var lbl = UIHelper.Lbl(label, 9.5f, FontStyle.Bold, UIHelper.TextMuted);
            lbl.Location = new Point(16, y); parent.Controls.Add(lbl); y += 26;
            tb = UIHelper.TB(ph); tb.Location = new Point(16, y); tb.Size = new Size(w-32, 40);
            parent.Controls.Add(tb); y += 54;
        }

        private void RefreshMyStatus()
        {
            if (_lblMyStatus == null) return;
            if (MyWaiting.MyItem == null || MyWaiting.MyRestaurant != _type)
            {
                _lblMyStatus.Text      = "등록된 대기가 없습니다.\n위에서 대기를 등록해 보세요.";
                _lblMyStatus.ForeColor = UIHelper.TextMuted;
                if (_btnCancelMy != null) _btnCancelMy.Visible = false;
            }
            else
            {
                int idx = _lv.Items.IndexOf(MyWaiting.MyItem);
                if (idx < 0)
                {
                    _lblMyStatus.Text      = "대기가 완료되었습니다! 🎉";
                    _lblMyStatus.ForeColor = _accent;
                    if (_btnCancelMy != null) _btnCancelMy.Visible = false;
                }
                else
                {
                    _lblMyStatus.Text      =
                        $"현재  {idx+1}번째  대기 중\n앞에 {idx}팀 남음\n예상 대기 약 {idx*15}분";
                    _lblMyStatus.ForeColor = UIHelper.TextPrimary;
                    if (_btnCancelMy != null) _btnCancelMy.Visible = true;
                }
            }
        }

        // ══════════════════════════════════════════
        //  초기 데이터 + 이벤트
        // ══════════════════════════════════════════
        private void LoadInitialData()
        {
            var rand = new Random();
            foreach (var (name, phone, party) in new[] {
                ("강은비","010-1111-1111",2),
                ("김수영","010-2222-2222",4),
                ("김영현","010-3333-3333",2),
            })
            {
                var ti  = new TimeItem($"{name}/{phone}", TimeSpan.FromSeconds(rand.Next(40,80)));
                var lvi = new ListViewItem(
                    new[]{ (_lv.Items.Count+1).ToString(), name, phone, $"{party}명","대기중" });
                lvi.Tag = ti;
                _lv.Items.Add(lvi);
            }
            _menu.UpdateWaitingCount(_type, _lv.Items.Count);
        }

        private void OnAddClicked(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_tbName.Text))
            { MessageBox.Show("이름을 입력해주세요."); return; }
            if (string.IsNullOrWhiteSpace(_tbPhone.Text))
            { MessageBox.Show("전화번호를 입력해주세요."); return; }

            var ti  = new TimeItem($"{_tbName.Text}/{_tbPhone.Text}",
                TimeSpan.FromSeconds(new Random().Next(40,80)));
            var lvi = new ListViewItem(
                new[]{ (_lv.Items.Count+1).ToString(), _tbName.Text, _tbPhone.Text,
                       _lblParty.Text, "대기중" });
            lvi.Tag = ti;
            _lv.Items.Add(lvi);
            MyWaiting.MyItem       = lvi;
            MyWaiting.MyRestaurant = _type;
            _tbName.Clear(); _tbPhone.Clear();
            _partySize = 2; _lblParty.Text = "2명";
            _menu.UpdateWaitingCount(_type, _lv.Items.Count);
            RefreshMyStatus();
            RefreshRanks();
        }

        private void OnDeleteClicked(object? s, EventArgs e)
        {
            if (_lv.FocusedItem == null)
            { MessageBox.Show("삭제할 항목을 선택하세요."); return; }
            _lv.Items.Remove(_lv.FocusedItem);
            _menu.UpdateWaitingCount(_type, _lv.Items.Count);
            RefreshMyStatus();
            RefreshRanks();
        }

        private void RefreshRanks()
        {
            for (int i = 0; i < _lv.Items.Count; i++)
                _lv.Items[i].SubItems[0].Text = (i+1).ToString();
        }

        private void StartTimer()
        {
            _timer.Interval = 1000;
            _timer.Tick += (s, e) =>
            {
                if (_lv.Items.Count == 0) return;
                var first = _lv.Items[0];
                if (first.Tag is TimeItem ti && DateTime.Now >= ti.ExpireTime)
                {
                    _lv.Items.RemoveAt(0);
                    _menu.UpdateWaitingCount(_type, _lv.Items.Count);
                    RefreshRanks();
                }
                if (MyWaiting.MyItem != null && MyWaiting.MyRestaurant == _type)
                {
                    int idx = _lv.Items.IndexOf(MyWaiting.MyItem);
                    if (idx == 0)
                    {
                        MyWaiting.MyItem = null;
                        RefreshMyStatus();
                        new WaitingAlarm(
                            "웨이팅 순서가 되었습니다!\n입장해 주세요. 🎉").ShowDialog();
                    }
                }
                RefreshMyStatus();
            };
            _timer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            base.OnFormClosed(e);
        }
    }
}
