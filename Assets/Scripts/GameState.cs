namespace Shurub
{
    public enum GameState
    {
        Invalid = -1,   // 이거 나오면 비상
        Lobby,          // 로비
        Boot,           // 게임 실행 직후
        Loading,        // 씬/리소스 로딩
        Ready,          // 카운트다운
        Playing,        // 실제 플레이
        Paused,         // 일시정지
        Result,         // 결과 화면
        Retry,          // 재시작
        GameOver,       // 완전 종료
    }
}
