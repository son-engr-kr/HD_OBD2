# rt android (X)
## 라즈베리파이 유니티 설치하고 실행 (RT android)
-https://www.youtube.com/watch?v=OzezRpA0O4A

# android os (O)
## 라즈베리파이 android os 설치 방법

https://beebom.com/how-install-android-raspberry-pi/

1.https://konstakang.com/devices/rpi4/ 들어가서 자기에게 필요한 android 버전 다운
2.https://etcher.balena.io/  sd카드로 옮기기 위해 etcher 다운


## 안드로이드에서 unity glitch 현상

https://answers.unity.com/questions/1077723/513-weird-graphic-glitch-on-some-android-devices.html

camera 설정에서 clear flag를 don't clear로 하면 된다는데 우린 안됨


### 해결 방법
Project Settings-Player-android tab-Other Settings

- Auto Graphics API 체크 해제
- Graphics APIs에 OpenGLES3만 포함
- Require ES~ 모두 체크 해제(문제 원인은 아니지만 체크해제상태로 가동 확인)
- Minimum API Level은 Android 11.0으로 되어있었음
- Mono-.NET Standard 2.1 (이러면 System.IO.Ports를 사용 못함)
- ARMv7 체크

## Unity에서 System.IO.Ports not found
https://stackoverflow.com/questions/37910933/error-while-working-with-unity-no-solution-worked-error-cs0234-the-type-or-n
- API Compatibillity level을 .Net Framework로 바꾸고 Window로 switch한 다음 Unity 껐다키면 된다.

# window iot core (X)
## windows iot 설명(gpio 쓰는 것도 나와있음)
https://learn.microsoft.com/en-us/windows/iot-core/tutorials/rpi
## windows iot-core 설치
https://learn.microsoft.com/en-us/windows/iot-core/downloads


디바이스 이름: hdobd2
관리자 비밀번호: 00000000


# 결국 Raspberrypi os로 진행해보기로 한다

공식 홈페이지
https://www.raspberrypi.com/software/

window용 설치파일 다운
- 64bit 라즈베리파이 os(full, lite는 GUI 없음)

## 유니티는 ARM Linux 빌드를 지원하지 않는다......

# 다시 돌아가서 윈도우로 진행해보기로 한다

- 먼저 bootloader를 업데이트
- https://blog.naver.com/PostView.naver?blogId=drangra&logNo=222438421433
- https://www.raspberrypi.com/software/
- 라즈베리파이 imager에서 Misc Utility image- bootloader-sdcard boot 선택하고
- sd카드를 라즈베리파이에 꽂고 화면을 연결하면 연두?초록? 화면이 뜬다. 그럼 끝
- sd카드 아무것도 안꽂고 켰을 때 GUI가 아니라 도스 화면이 뜬다.

- https://thesecmaster.com/step-by-step-procedure-to-install-windows-11-on-a-raspberry-pi-4/
- https://linuxhint.com/install-windows-10-raspberry-pi-4/ (좀 더 자세함)

-> https://uupdump.net/ 에서 **"Windows 11, version 22H2 (22621.1778) arm64"** 를 다운받음

-> windows pro

-> download and convert to ISO, include updates

**반드시!! 백신, 윈도우 실시간 디펜스를 끄고 해야한다.**

# 안드로이드에서 VID, PID를 manifest에 명시해주면 되려나?

VID, PID 찾는 법 

https://kb.synology.com/ko-kr/DSM/tutorial/How_do_I_check_the_PID_VID_of_my_USB_device#:~:text=%EC%A0%9C%EC%96%B4%ED%8C%90%20%3E%20%EC%9E%A5%EC%B9%98%20%EA%B4%80%EB%A6%AC%EC%9E%90%EB%A1%9C%20%EC%9D%B4%EB%8F%99,%EC%99%80%20VID%EB%A5%BC%20%ED%99%95%EC%9D%B8%ED%95%A9%EB%8B%88%EB%8B%A4.

장치관리자-포트-UNO-속성-자세히-하드웨어 ID

우리가 사용한 우노: VID_2341, PID_0043, REV_0001?

Unity Build 오류에 대해서는
https://forum.unity.com/threads/exception-obsolete-providing-android-resources-in-assets-plugins-android-assets-was-removed.1171829/
의 skeyll 답변을 참조. 