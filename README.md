# 🍽️ WaiRes — 웨이팅 및 예약 관리 앱

대형 건물 내 입점한 여러 식당(아웃백, 토끼정, 빕스, 쉑쉑버거)의 실시간 웨이팅 상태를 확인하고,
사용자 정보 입력을 통해 대기 순번 및 예약(날짜·시간·인원)을 관리하는 **C# WinForms 데스크톱 애플리케이션**입니다.

> 캐치테이블의 UX를 참고하여 디자인했습니다.

<!-- 여기에 메인 화면 스크린샷 1장을 넣으면 가장 효과적입니다 -->
<!-- ![메인 화면](docs/images/main.png) -->

---

## ✨ 주요 기능

### 1. 웨이팅(대기열) 시스템
- 4개 매장 카드에서 실시간 대기 인원 표시 (`WaitingMenu`)
- 개별 매장 입장 시 대기 등록 및 타이머 기반 순번 관리 (`WaitingForm`)
- 내 순서가 다가오면 알림 팝업으로 안내 (`WaitingAlarm`)

### 2. 예약 시스템
- 매장 선택 → 날짜/시간/인원 선택 → 정보 입력의 멀티 스텝 플로우 (`ReservationMenu`, `ReservationForm`)
- 내가 만든 예약 목록 조회 및 취소 (`CheckReservation`)

<!-- 여기에 기능별 스크린샷 2~4장을 GIF 또는 이미지로 추가하면 좋습니다 -->
<!--
| 대기열 | 예약 플로우 |
|---|---|
| ![웨이팅](docs/images/waiting.png) | ![예약](docs/images/reservation.png) |
-->

---

## 🛠 사용 기술

| 분류 | 내용 |
|---|---|
| 언어 | C# |
| 프레임워크 | .NET 6.0, WinForms |
| UI 구현 | Code-based UI (절대좌표 배치, GDI+ Custom Paint) |
| 저장 방식 | 정적 클래스 기반 인메모리 스토어 (`ReservationStore`, `MyWaiting`) |

---

## 🏗 아키텍처

```
Waiting_Reservation/
├── Program.cs              # 앱 진입점
├── Form1.cs                # 메인 화면 (웨이팅/예약 메뉴 진입)
├── UIHelper.cs              # 공용 디자인 유틸 (색상/폰트 팔레트, 공통 드로잉)
│
├── WaitingMenu.cs           # 매장별 대기 카드 목록
├── WaitingForm.cs           # 개별 매장 대기 등록/순번 화면
├── WaitingAlarm.cs          # 대기 알림 팝업
│
├── ReservationMenu.cs        # 매장별 예약 카드 목록
├── ReservationForm.cs        # 예약 단계별(날짜→시간→인원→정보) 입력 화면
├── CheckReservation.cs       # 내 예약 목록 조회/삭제
│
└── classs/
    ├── EnumType.cs           # RestaurantType enum (Outback, Jung, Vips, SS)
    ├── ReservationItem.cs     # 예약 데이터 모델 + ReservationStore
    └── TimeItem.cs            # 대기 시간 만료 체크용 모델
```

### 핵심 설계 포인트
- **DockStyle 대신 절대좌표 배치**: WinForms 커스텀 UI에서 DockStyle 기반 레이아웃이 불안정하여, 좌표 직접 계산 방식으로 전환했습니다.
- **`Shown` 이벤트에서 레이아웃 계산**: 생성자/`Load` 시점에는 `ClientSize`가 정확하지 않아 `Shown` 이벤트에서 위치를 재계산합니다.
- **`Paint` 이벤트 기반 직접 드로잉**: 그라데이션 패널 위 투명 Label은 ClearType 렌더링이 깨지는 문제가 있어, `DrawString`으로 직접 그리는 방식을 사용했습니다.
- **Hide/Show 기반 화면 전환**: Close/Open이 아닌 Hide/Show로 폼 간 이동하여 상태를 유지합니다.
- **Dictionary 캐싱**: 폼 인스턴스와 배지 Label을 캐싱해 중복 생성을 방지합니다.

---

## ▶️ 실행 방법

```bash
git clone https://github.com/zhsks5298-code/Waiting-Reservation.git
cd Waiting-Reservation
dotnet build
dotnet run --project Waiting_Reservation.csproj
```

또는 Visual Studio에서 `Waiting_Reservation.csproj`를 열어 바로 실행할 수 있습니다.

---

## 📌 트러블슈팅 / 배운 점

- DockStyle 레이아웃의 한계와 절대좌표 배치로의 전환 과정
- 투명 Label + 그라데이션 배경에서 발생한 ClearType 렌더링 깨짐 문제 해결
- 카드 비율/폰트 크기 일관성 확보를 위한 반복적 UI 디버깅

---

## 📷 스크린샷

<!-- 아래 자리에 실제 실행 화면 캡쳐를 추가하세요 (4~6장 권장) -->
<!--
![메인](docs/images/main.png)
![웨이팅 메뉴](docs/images/waiting-menu.png)
![예약 단계 1](docs/images/reservation-step1.png)
![내 예약 목록](docs/images/my-reservations.png)
-->
