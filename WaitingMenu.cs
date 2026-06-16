using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Waiting_Reservation.classs;

namespace Waiting_Reservation
{
    public class WaitingMenu : Form
    {
        private readonly Form _parent;
        private readonly Dictionary<RestaurantType, Label>       _countLabels = new();
        private readonly Dictionary<RestaurantType, WaitingForm> _forms       = new();

        private static readonly RestaurantType[] RESTAURANTS =
        {
            RestaurantType.Outback, RestaurantType.Jung,
            RestaurantType.Vips,    RestaurantType.SS
        };

        private const int HEADER_H = 104;
        private const int GAP      = 16;  // 카드 사이 간격

        public WaitingMenu(Form parent)
        {
            _parent         = parent;
            Text            = "웨이팅 등록";
            BackColor       = UIHelper.BgColor;
            Size            = new Size(900, 820);
            MinimumSize     = new Size(900, 820);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;

            Build();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _parent?.Show();
        }

        private void Build()
        {
            int cw = ClientSize.Width;
            int ch = ClientSize.Height;

            // ── 헤더 (절대좌표, DockStyle 없음)
            var header = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(cw, HEADER_H),
                BackColor = Color.FromArgb(192, 57, 43),
            };
            header.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var br = new LinearGradientBrush(header.ClientRectangle,
                    Color.FromArgb(210, 70, 20), Color.FromArgb(160, 40, 10),
                    LinearGradientMode.Horizontal);
                g.FillRectangle(br, header.ClientRectangle);
                using var tf = new Font(UIHelper.FONT, 17f, FontStyle.Bold);
                g.DrawString("⏳  웨이팅 등록", tf, Brushes.White, 108f, 24f);
                using var sf = new Font(UIHelper.FONT, 9.5f);
                g.DrawString("방문할 레스토랑을 선택하세요", sf, Brushes.White, 112f, 68f);
            };

            var btnBack = UIHelper.Btn("↩", Color.Transparent, Color.White, 14f);
            btnBack.Location = new Point(14, 22); btnBack.Size = new Size(80, 32);
            btnBack.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, Color.White);
            btnBack.Click   += (s, e) => Close();
            header.Controls.Add(btnBack);


            Controls.Add(header);

            // ── 카드 영역 (헤더 아래, 절대좌표)
            int gridTop  = HEADER_H;
            int gridH    = ch - gridTop;
            int gridW    = cw;

            int pad      = 20;                          // 외곽 여백
            int colGap   = GAP;
            int rowGap   = GAP;
            int cardW    = (gridW - pad * 2 - colGap) / 2;
            int cardH    = (gridH - pad * 2 - rowGap) / 2;

            for (int i = 0; i < RESTAURANTS.Length; i++)
            {
                int col  = i % 2;
                int row  = i / 2;
                int cx   = pad + col * (cardW + colGap);
                int cy   = gridTop + pad + row * (cardH + rowGap);

                var card = BuildCard(RESTAURANTS[i], cx, cy, cardW, cardH);
                Controls.Add(card);
            }
        }

        private Panel BuildCard(RestaurantType t, int x, int y, int w, int h)
        {
            var accent = UIHelper.GetAccent(t);

            var card = new Panel
            {
                Location  = new Point(x, y),
                Size      = new Size(w, h),
                BackColor = UIHelper.BgColor,
                Cursor    = Cursors.Hand,
            };
            card.Paint += (s, e) =>
                UIHelper.PaintCard(e.Graphics, new Rectangle(0, 0, w, h), 14, UIHelper.CardColor);

            // ── 이모지 배경 (카드 상단 60%)
            int emojiH = (int)(h * 0.50);
            var emojiArea = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(w, emojiH),
                BackColor = UIHelper.GetLightBg(t),
            };
            emojiArea.Paint += (s, e) =>
            {
                var p = (Panel)s;
                UIHelper.PaintCard(e.Graphics,
                    new Rectangle(0, 0, p.Width, p.Height), 14,
                    UIHelper.GetLightBg(t), UIHelper.GetLightBg(t));

                using var f  = new Font("Segoe UI Emoji", 36f);
                string   em  = UIHelper.GetEmoji(t);
                var      sz  = e.Graphics.MeasureString(em, f);
                e.Graphics.DrawString(em, f, Brushes.Black,
                    (p.Width  - sz.Width)  / 2f,
                    (p.Height - sz.Height) / 2f);
            };
            card.Controls.Add(emojiArea);

            // ── 텍스트 영역
            int infoY = emojiH + 16;
            var lblName = new Label
            {
                Text      = UIHelper.GetName(t),
                Font      = new Font(UIHelper.FONT, 12f, FontStyle.Bold),
                ForeColor = UIHelper.TextPrimary,
                BackColor = Color.White,
                AutoSize  = false,
                Size      = new Size(w - 24, 32),
                Location  = new Point(12, infoY),
            };
            card.Controls.Add(lblName);

            var lblCat = new Label
            {
                Text      = UIHelper.GetCategory(t),
                Font      = new Font(UIHelper.FONT, 9f),
                ForeColor = UIHelper.TextMuted,
                BackColor = Color.White,
                AutoSize  = false,
                Size      = new Size(w - 24, 24),
                Location  = new Point(12, infoY + 38),
            };
            card.Controls.Add(lblCat);

            // 대기 수 뱃지
            var lblCount = new Label
            {
                Text      = "-팀 대기",
                Font      = new Font(UIHelper.FONT, 9f, FontStyle.Bold),
                ForeColor = accent,
                BackColor = Color.FromArgb(30, accent),
                AutoSize  = false,
                Size      = new Size(w - 24, 26),
                Location  = new Point(12, infoY + 70),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            UIHelper.SetRounded(lblCount, 6);
            _countLabels[t] = lblCount;
            card.Controls.Add(lblCount);

            // 클릭 이벤트
            EventHandler onClick = (s, e) =>
            {
                if (!_forms.ContainsKey(t))
                    _forms[t] = new WaitingForm(t, this);
                Hide();
                _forms[t].Show();
            };
            card.Click      += onClick;
            emojiArea.Click += onClick;
            lblName.Click   += onClick;
            lblCat.Click    += onClick;
            lblCount.Click  += onClick;

            return card;
        }

        public void UpdateWaitingCount(RestaurantType t, int count)
        {
            if (_countLabels.TryGetValue(t, out var lbl))
                lbl.Text = $"현재 {count}팀 대기";
        }
    }
}
