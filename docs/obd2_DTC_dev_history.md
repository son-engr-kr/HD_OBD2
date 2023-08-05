## ELM327 이란
- 자동차에 탑재된 OBD 인터페이스를 번역하기 위해 ELM Electronics에서 제조한 프로그래밍 된 마이크로 컨트롤러이다.

## What is Freeze Frame?
-ECU가 고장이 감지 되었을때의 변수들의 값을 스냅샷 으로 찍은것.

## Freeze Frame 사용이유
- 만약 하나의 오류로 인해서 동시에 여러개의 DTC가 발생하면 우리는 그 DTC중 뭐가 진짜 원인인지 구분하기 어렵지만 Freeze Frame을 통해 진짜 원인가 가장 먼저 일어난 오류에 대해 정보를 준다.

## OBD2를 통해 Freeze Frame를 받아 오는법
- mode에 0x02, pid에 0x02를 사용한다. (안된다면 십진법 02,02를 사용하자)

## OBD2를 통해 문제 진단 코드(DTC)를 받아 오는법
OBD2 Frame 에는 mode를 선택하는 바이트가 있다.현재 우리가 rpm값, 속도 값,냉각수 온도 값등 현재 데이터를 볼려고 할때 우리는 mode에 0x01를 썼지만 저장된 DTC를 쓸때는 mode를 0x03으로 확인 가능하다.

-mode에 0x03, pid에 0x00을 사용한다. (위키피디아에는 DTC를 받아오는 pid는 해당사항없음이라고 표기 되어있음--->자료를 찾아보니 0x00사용해야 함)

-진단코드 1개당 2byte의 메모리가 필요하다.(OBD Frame 하나당 최대 2개의 문제진단코드를 넣을 수 있음.) 

즉, 16bit가 필요한데 상위 2bit가 (00이면 P-powertrain),(01이면 C-chassis[샤시]), (10이면 B-body),(11이면 U-Network) 

다음 상위 2bit가 (00이면 SAE),(01이면 생산업체)

남은 12bit는 4bit씩 코드 하나를 차지한다.<br>

![img](https://img1.daumcdn.net/thumb/R1280x0/?scode=mtistory2&fname=https%3A%2F%2Ft1.daumcdn.net%2Ftistoryfile%2Ffs8%2F16_tistory_2008_08_20_09_01_48ab5ecf14823%3Foriginal)