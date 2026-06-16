using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Waiting_Reservation.classs;

namespace Waiting_Reservation
{
    /// <summary>앱 전체 공용 디자인 유틸</summary>
    public static class UIHelper
    {
        // ── 폰트 (Malgun Gothic 는 한국 윈도우에 기본 내장)
        public const string FONT = "Malgun Gothic";  // Noto Sans KR 설치 시 변경 가능

        // ── 색상 팔레트
        public static readonly Color BgColor    = Color.FromArgb(248, 246, 242);
        public static readonly Color CardColor  = Color.White;
        public static readonly Color BorderColor= Color.FromArgb(228, 224, 218);
        public static readonly Color TextPrimary= Color.FromArgb(26, 26, 26);
        public static readonly Color TextMuted  = Color.FromArgb(140, 140, 140);
        public static readonly Color Danger     = Color.FromArgb(192, 57, 43);

        // ── 식당 정보
        public static Color GetAccent(RestaurantType t) => t switch
        {
            RestaurantType.Jung    => Color.FromArgb(44,  122,  75),
            RestaurantType.Outback => Color.FromArgb(192,  57,  43),
            RestaurantType.Vips    => Color.FromArgb(26,   82, 118),
            RestaurantType.SS      => Color.FromArgb(120,  66,  18),
            _ => Color.FromArgb(255, 107, 53)
        };
        public static Color GetLightBg(RestaurantType t) => t switch
        {
            RestaurantType.Jung    => Color.FromArgb(235, 248, 240),
            RestaurantType.Outback => Color.FromArgb(252, 235, 233),
            RestaurantType.Vips    => Color.FromArgb(232, 241, 250),
            RestaurantType.SS      => Color.FromArgb(250, 242, 233),
            _ => Color.FromArgb(255, 244, 238)
        };
        public static string GetName(RestaurantType t) => t switch
        {
            RestaurantType.Jung    => "토끼정",
            RestaurantType.Outback => "아웃백",
            RestaurantType.Vips    => "빕스",
            RestaurantType.SS      => "쉑쉑버거",
            _ => ""
        };
        public static string GetCategory(RestaurantType t) => t switch
        {
            RestaurantType.Jung    => "한식 · 고기구이",
            RestaurantType.Outback => "스테이크 · 패밀리",
            RestaurantType.Vips    => "뷔페 · 패밀리",
            RestaurantType.SS      => "버거 · 패스트푸드",
            _ => ""
        };
        public static string GetEmoji(RestaurantType t) => t switch
        {
            RestaurantType.Jung    => "🐰",
            RestaurantType.Outback => "🥩",
            RestaurantType.Vips    => "🍽",
            RestaurantType.SS      => "🍔",
            _ => "🍴"
        };

        // ── 그리기 헬퍼
        public static GraphicsPath RoundedPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            p.StartFigure();
            p.AddArc(r.X,           r.Y,           d, d, 180, 90);
            p.AddArc(r.Right - d,   r.Y,           d, d, 270, 90);
            p.AddArc(r.Right - d,   r.Bottom - d,  d, d,   0, 90);
            p.AddArc(r.X,           r.Bottom - d,  d, d,  90, 90);
            p.CloseFigure();
            return p;
        }

        public static void PaintCard(Graphics g, Rectangle rect, int radius,
            Color fill, Color border = default, float bw = 1.5f)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (border == default) border = BorderColor;
            using var path = RoundedPath(rect, radius);
            using var br   = new SolidBrush(fill);
            g.FillPath(br, path);
            using var pen  = new Pen(border, bw);
            g.DrawPath(pen, path);
        }

        public static void SetRounded(Control c, int radius = 10)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            p.StartFigure();
            p.AddArc(0,          0,           d, d, 180, 90);
            p.AddArc(c.Width-d,  0,           d, d, 270, 90);
            p.AddArc(c.Width-d,  c.Height-d,  d, d,   0, 90);
            p.AddArc(0,          c.Height-d,  d, d,  90, 90);
            p.CloseFigure();
            c.Region = new Region(p);
        }

        // ── 컨트롤 팩토리
        public static Button Btn(string text, Color back, Color fore = default, float sz = 10f)
        {
            if (fore == default) fore = Color.White;
            var b = new Button {
                Text = text, BackColor = back, ForeColor = fore,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(FONT, sz, FontStyle.Bold),
                Cursor = Cursors.Hand, UseVisualStyleBackColor = false,
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(back, 0.08f);
            b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(back, 0.18f);
            return b;
        }

        public static Label Lbl(string text, float sz = 10f,
            FontStyle style = FontStyle.Regular, Color col = default)
        {
            if (col == default) col = TextPrimary;
            return new Label {
                Text = text, AutoSize = true, BackColor = Color.Transparent,
                Font = new Font(FONT, sz, style), ForeColor = col,
            };
        }

        public static TextBox TB(string placeholder = "", int height = 38)
        {
            return new TextBox {
                PlaceholderText = placeholder, BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font(FONT, 11f), Height = height,
            };
        }

        /// <summary>배경색이 BgColor인 라운드 흰색 카드 패널</summary>
        public static Panel CardPanel(int x, int y, int w, int h, int r = 12)
        {
            var p = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = BgColor };
            p.Paint += (s, e) => PaintCard(e.Graphics, new Rectangle(0,0,p.Width,p.Height), r, CardColor);
            return p;
        }

        /// <summary>LinearGradient 헤더 패널</summary>
        public static Panel GradientHeader(int height, Color c1, Color c2, DockStyle dock = DockStyle.Top)
        {
            var p = new Panel { Height = height, Dock = dock };
            p.Paint += (s, e) => {
                using var br = new LinearGradientBrush(p.ClientRectangle, c1, c2, LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(br, p.ClientRectangle);
            };
            return p;
        }
    }
}
