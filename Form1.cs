using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Waiting_Reservation
{
    public class Form1 : Form
    {
        public Form1()
        {
            Text             = "WaiRes";
            BackColor        = UIHelper.BgColor;
            Size             = new Size(520, 780);
            MinimumSize      = new Size(520, 780);
            StartPosition    = FormStartPosition.CenterScreen;
            FormBorderStyle  = FormBorderStyle.FixedSingle;
            MaximizeBox      = false;
            Font             = new Font(UIHelper.FONT, 9.5f);

            Build();
        }

        private void Build()
        {
            // ClientSize 는 Form 크기에서 테두리·타이틀바 제외한 실제 영역
            int cw = ClientSize.Width;         // ≈ 504
            int bw = cw - 64;                  // 버튼 너비 (양쪽 32px 여백)
            int bx = 32;                       // 버튼 x 시작

            // ════════════════════════════════════════
            //  ① 파란 상단 패널 (절대 좌표, DockStyle 없음)
            // ════════════════════════════════════════
            var bluePanel = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(cw, 360),
                BackColor = Color.FromArgb(26, 26, 46),
            };
            bluePanel.Paint += (s, e) =>
            {
                var p = (Panel)s;
                using var br = new LinearGradientBrush(
                    p.ClientRectangle,
                    Color.FromArgb(22, 24, 48),
                    Color.FromArgb(50, 68, 112),
                    LinearGradientMode.ForwardDiagonal);
                e.Graphics.FillRectangle(br, p.ClientRectangle);
            };

            // 앱 이름
            bluePanel.Controls.Add(new Label
            {
                Text      = "WaiRes",
                Font      = new Font(UIHelper.FONT, 26f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize  = false,
                Size      = new Size(cw, 60),
                Location  = new Point(0, 44),
                TextAlign = ContentAlignment.MiddleCenter,
            });

            // 부제목
            bluePanel.Controls.Add(new Label
            {
                Text      = "웨이팅 · 예약 서비스",
                Font      = new Font(UIHelper.FONT, 11f),
                ForeColor = Color.FromArgb(170, 195, 228),
                BackColor = Color.Transparent,
                AutoSize  = false,
                Size      = new Size(cw, 28),
                Location  = new Point(0, 112),
                TextAlign = ContentAlignment.MiddleCenter,
            });

            // 포크 아이콘
            bluePanel.Controls.Add(new Label
            {
                Text      = "🍴",
                Font      = new Font("Segoe UI Emoji", 52f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize  = false,
                Size      = new Size(cw, 160),
                Location  = new Point(0, 158),
                TextAlign = ContentAlignment.MiddleCenter,
            });

            Controls.Add(bluePanel);

            // ════════════════════════════════════════
            //  ② 안내 텍스트
            // ════════════════════════════════════════
            Controls.Add(new Label
            {
                Text      = "서비스를 선택하세요",
                Font      = new Font(UIHelper.FONT, 12f, FontStyle.Bold),
                ForeColor = UIHelper.TextPrimary,
                BackColor = UIHelper.BgColor,
                AutoSize  = false,
                Size      = new Size(cw, 28),
                Location  = new Point(0, 380),
                TextAlign = ContentAlignment.MiddleCenter,
            });

            // ════════════════════════════════════════
            //  ③ 웨이팅 버튼
            // ════════════════════════════════════════
            var btnW = UIHelper.Btn("⏳   웨이팅 등록",
                Color.FromArgb(220, 85, 20), Color.White, 13f);
            btnW.Size     = new Size(bw, 76);
            btnW.Location = new Point(bx, 424);
            btnW.Click   += (s, e) => { var wm = new WaitingMenu(this); Hide(); wm.Show(); };
            UIHelper.SetRounded(btnW, 12);
            Controls.Add(btnW);

    

            // ════════════════════════════════════════
            //  ④ 예약 버튼
            // ════════════════════════════════════════
            var btnR = UIHelper.Btn("📅   예약 하기",
                Color.FromArgb(32, 54, 120), Color.White, 13f);
            btnR.Size     = new Size(bw, 76);
            btnR.Location = new Point(bx, 544);
            btnR.Click   += (s, e) => { var rm = new ReservationMenu(this); Hide(); rm.Show(); };
            UIHelper.SetRounded(btnR, 12);
            Controls.Add(btnR);

            
        }
    }
}
