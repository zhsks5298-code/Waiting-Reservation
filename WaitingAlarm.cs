using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Windows.Forms;

namespace Waiting_Reservation
{
    public class WaitingAlarm : Form
    {
        public WaitingAlarm(string message)
        {
            Text            = "웨이팅 알림";
            Size            = new Size(460, 340);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = Color.White;
            TopMost         = true;

            SystemSounds.Exclamation.Play();

            // 상단 색상 바
            var topBar = new Panel
            {
                Height    = 6,
                Dock      = DockStyle.Top,
                BackColor = Color.FromArgb(220, 85, 20),
            };
            Controls.Add(topBar);

            // 벨 아이콘
            var lblBell = new Label
            {
                Text      = "🔔",
                Font      = new Font("Segoe UI Emoji", 32f),
                AutoSize  = false,
                Size      = new Size(460, 80),
                Location  = new Point(0, 16),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
            };
            Controls.Add(lblBell);

            // 메시지 (여러 줄 대응)
            var lblMsg = new Label
            {
                Text      = message,
                Font      = new Font(UIHelper.FONT, 12f, FontStyle.Bold),
                AutoSize  = false,
                Size      = new Size(400, 100),
                Location  = new Point(30, 104),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = UIHelper.TextPrimary,
                BackColor = Color.Transparent,
            };
            Controls.Add(lblMsg);

            // 확인 버튼
            var btn = UIHelper.Btn("확인", Color.FromArgb(220, 85, 20));
            btn.Size     = new Size(200, 50);
            btn.Location = new Point(130, 226);
            UIHelper.SetRounded(btn, 10);
            btn.Click   += (s, e) => Close();
            Controls.Add(btn);
        }
    }
}
