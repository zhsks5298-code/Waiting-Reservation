using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Waiting_Reservation.classs;

namespace Waiting_Reservation
{
    public class ReservationMenu : Form
    {
        private readonly Form _parent;
        private const int HEADER_H = 104;

        private static readonly RestaurantType[] RESTAURANTS =
        {
            RestaurantType.Outback, RestaurantType.Jung,
            RestaurantType.Vips,    RestaurantType.SS
        };

        public ReservationMenu(Form parent)
        {
            _parent         = parent;
            Text            = "예약 하기";
            BackColor       = UIHelper.BgColor;
            Size            = new Size(900, 840);
            MinimumSize     = new Size(900, 840);
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

            // ── 헤더 (절대좌표)
            var header = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(cw, HEADER_H),
                BackColor = Color.FromArgb(32, 54, 120),
            };
            header.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var br = new LinearGradientBrush(header.ClientRectangle,
                    Color.FromArgb(32, 54, 120), Color.FromArgb(16, 32, 80),
                    LinearGradientMode.Horizontal);
                g.FillRectangle(br, header.ClientRectangle);
                using var tf = new Font(UIHelper.FONT, 17f, FontStyle.Bold);
                g.DrawString("📅  예약 하기", tf, Brushes.White, 108f, 16f);
                using var sf = new Font(UIHelper.FONT, 9.5f);
                g.DrawString("예약할 레스토랑을 선택하세요", sf, Brushes.White, 112f, 68f);
            };

            var btnBack = UIHelper.Btn("↩", Color.Transparent, Color.White, 14f);
            btnBack.Location = new Point(14, 22); btnBack.Size = new Size(80, 30);
            btnBack.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, Color.White);
            btnBack.Click   += (s, e) => Close();
            header.Controls.Add(btnBack);

            Controls.Add(header);

            // ── 카드 4개 (절대좌표 — DockStyle 없음)
            int btnAreaH = 66;          // 하단 "내 예약 확인" 버튼 영역
            int gridTop  = HEADER_H;
            int gridH    = ch - gridTop - btnAreaH;

            int pad    = 18;
            int colGap = 14;
            int rowGap = 14;
            int cardW  = (cw - pad * 2 - colGap) / 2;
            int cardH  = (gridH - pad * 2 - rowGap) / 2;

            for (int i = 0; i < RESTAURANTS.Length; i++)
            {
                int col = i % 2;
                int row = i / 2;
                int cx  = pad + col * (cardW + colGap);
                int cy  = gridTop + pad + row * (cardH + rowGap);

                Controls.Add(BuildCard(RESTAURANTS[i], cx, cy, cardW, cardH));
            }

            // ── 내 예약 확인 버튼 (하단 고정)
            var btnCheck = UIHelper.Btn("📋  내 예약 확인",
                Color.FromArgb(32, 54, 120));
            btnCheck.Size     = new Size(cw - 40, 50);
            btnCheck.Location = new Point(20, ch - btnAreaH + 8);
            UIHelper.SetRounded(btnCheck, 10);
            btnCheck.Click += (s, e) => new CheckReservation().ShowDialog(this);
            Controls.Add(btnCheck);
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

            // 이모지 배경 (카드 상단 58%)
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
                using var f  = new Font("Segoe UI Emoji", 34f);
                string   em  = UIHelper.GetEmoji(t);
                var      sz  = e.Graphics.MeasureString(em, f);
                e.Graphics.DrawString(em, f, Brushes.Black,
                    (p.Width  - sz.Width)  / 2f,
                    (p.Height - sz.Height) / 2f);
            };
            card.Controls.Add(emojiArea);

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

            var lblBadge = new Label
            {
                Text      = "예약 가능",
                Font      = new Font(UIHelper.FONT, 9f, FontStyle.Bold),
                ForeColor = accent,
                BackColor = Color.FromArgb(28, accent),
                AutoSize  = false,
                Size      = new Size(w - 24, 30),
                Location  = new Point(12, infoY + 70),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            UIHelper.SetRounded(lblBadge, 6);
            card.Controls.Add(lblBadge);

            EventHandler onClick = (s, e) => OpenReservation(t);
            card.Click      += onClick;
            emojiArea.Click += onClick;
            lblName.Click   += onClick;
            lblCat.Click    += onClick;
            lblBadge.Click  += onClick;

            return card;
        }

        private void OpenReservation(RestaurantType t)
        {
            var f = new ReservationForm(t, this);
            Hide();
            f.Show();
        }
    }
}
