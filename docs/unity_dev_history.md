# UIToolkit

## 기본 Style과 transition Style 중 어떤걸?
기본 Style을 uxml에서 정의하고 transition을 줄 부분만 uss파일에서 정의해주면 좋을 것 같은데 Style을 extract를 하면 기본 Style이 다 옮겨져 버린다..

## Style Class에도 우선순위가 있다.
나중에 정의된 것이 먼저 정의된 class를 override 한다.
즉, Show를 정의하고 Hide를 정의한 다음
Show,Hide를 둘 다 apply하면 Hide가 Show를 Override 함.
(apply 순서는 상관 없음)

# 해보고 싶은거
- 가상 배기음(RPM에 따른) -> 옵션에서 키고 끄기
- 속도 높아지면 차 멀리서 보기 ( 속도에 따라 ViewPositionObject 뒤로 이동 )
- 계기판
- HUD