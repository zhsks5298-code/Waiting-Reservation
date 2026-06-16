using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Waiting_Reservation.classs;

namespace Waiting_Reservation
{
    public class CheckReservation : Form
    {
        private ListView _lv        = null!;
        private Label    _lblEmpty  = null!;
        private Button   _btnDel    = null!;

        private const int HEADER_H = 78;
        private const int FOOTER_H = 62;

        public CheckReservation()
        {
            Text            = "내 예약 목록";
            Size            = new Size(980, 680);
            MinimumSize     = new Size(980, 680);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            BackColor       = UIHelper.BgColor;
            Font            = new Font(UIHelper.FONT, 9.5f);

            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            int cw = ClientSize.Width;
            int ch = ClientSize.Height;

            // ── 헤더 (절대좌표 + Paint 에서 텍스트 그림)
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
                using var tf = new Font(UIHelper.FONT, 15f, FontStyle.Bold);
                g.DrawString("📋  내 예약 목록", tf, Brushes.White, 22f, 20f);
            };
            Controls.Add(header);

            // ── 하단 버튼 (절대좌표)
            var footer = new Panel
            {
                Location  = new Point(0, ch - FOOTER_H),
                Size      = new Size(cw, FOOTER_H),
                BackColor = Color.White,
            };

            _btnDel = UIHelper.Btn("선택한 예약 취소", UIHelper.Danger);
            _btnDel.Size = new Size(200, 44); _btnDel.Location = new Point(18, 9);
            UIHelper.SetRounded(_btnDel, 8);
            _btnDel.Click += OnCancel;
            footer.Controls.Add(_btnDel);

            var btnClose = UIHelper.Btn("닫기", UIHelper.BorderColor, UIHelper.TextPrimary);
            btnClose.Size = new Size(120, 44);
            btnClose.Location = new Point(cw - 140, 9);
            UIHelper.SetRounded(btnClose, 8);
            btnClose.Click += (s, e) => Close();
            footer.Controls.Add(btnClose);
            Controls.Add(footer);

            // ── ListView (헤더 아래 ~ 푸터 위, 절대좌표)
            int lvY = HEADER_H;
            int lvH = ch - HEADER_H - FOOTER_H;

            _lv = new ListView
            {
                Location      = new Point(0, lvY),
                Size          = new Size(cw, lvH),
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                BackColor     = Color.White,
                Font          = new Font(UIHelper.FONT, 9.5f),
                BorderStyle   = BorderStyle.None,
                HeaderStyle   = ColumnHeaderStyle.Nonclickable,
            };

            // 폼 너비(980) 기준 컬럼 배분
            _lv.Columns.Add("식당",      100);
            _lv.Columns.Add("날짜",      148);
            _lv.Columns.Add("시간",       76);
            _lv.Columns.Add("인원",       56);
            _lv.Columns.Add("예약자",    100);
            _lv.Columns.Add("연락처",    148);
            _lv.Columns.Add("요청사항",  300);  // 나머지 공간
            Controls.Add(_lv);

            // ── 예약 없음 레이블
            _lblEmpty = new Label
            {
                Text      = "아직 예약 내역이 없습니다.\n예약 화면에서 예약을 진행해 보세요!",
                AutoSize  = false,
                Size      = new Size(440, 64),
                Location  = new Point((cw - 440) / 2, lvY + (lvH - 64) / 2),
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font(UIHelper.FONT, 11f),
                ForeColor = UIHelper.TextMuted,
                BackColor = UIHelper.BgColor,
                Visible   = false,
            };
            Controls.Add(_lblEmpty);
        }

        private void LoadData()
        {
            _lv.Items.Clear();
            bool empty = ReservationStore.Items.Count == 0;
            _lv.Visible      = !empty;
            _lblEmpty.Visible =  empty;
            _btnDel.Enabled   = !empty;

            if (empty) return;

            foreach (var r in ReservationStore.Items)
            {
                var lvi = new ListViewItem(new[]
                {
                    r.RestaurantName,
                    r.Date.ToString("MM월 dd일 (ddd)"),
                    r.Time,
                    $"{r.PartySize}명",
                    r.Name,
                    r.Phone,
                    string.IsNullOrWhiteSpace(r.Request) ? "-" : r.Request,
                });
                lvi.Tag = r.Id;
                _lv.Items.Add(lvi);
            }
        }

        private void OnCancel(object? sender, EventArgs e)
        {
            if (_lv.SelectedItems.Count == 0)
            { MessageBox.Show("취소할 예약을 선택하세요.", "알림"); return; }

            if (MessageBox.Show(
                $"선택한 {_lv.SelectedItems.Count}건의 예약을 취소하시겠습니까?",
                "예약 취소", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes)
            {
                foreach (ListViewItem lvi in _lv.SelectedItems)
                    ReservationStore.Remove((int)lvi.Tag!);
                LoadData();
            }
        }
    }
}
