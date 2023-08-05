# GPT API를 UNITY에서 사용하는 법
1.OpenAI 계정을 만든다.<br>
-https://platform.openai.com/overview <br>

2.OpenAI-Unity Package를 import한다.<br>
-https://github.com/srcnalt/OpenAI-Unity <br>

3.local credentials를 설정한다.<br>
-홈디렉토리에 가서 [.openai]라는 폴더를 만든다.<br>
-[.openai]폴더 안에 텍스트 파일을 만들어 {"api_key": "abcd"}를 붙여놓고 abcd대신에 자신의 키를 붙여 넣는다.<br>
-저장할때 .json 파일로 저장한다.

4.GPT스크립트에서 prompt변수에 gpt에게 부여할 역할을 설정한다.

![img](https://img1.daumcdn.net/thumb/R1280x0/?scode=mtistory2&fname=https%3A%2F%2Fblog.kakaocdn.net%2Fdn%2FYS19l%2Fbtsg9DnUPqv%2F0EbnLrkJYBEEjzTnyn2fDk%2Fimg.png)

5.역할을 부여하고 테스트
![img](https://img1.daumcdn.net/thumb/R1280x0/?scode=mtistory2&fname=https%3A%2F%2Fblog.kakaocdn.net%2Fdn%2FoQH2p%2Fbtsg95Les9v%2FqIi7sVK4ViPNvKPw6ELha0%2Fimg.png)

참고 영상 : https://www.youtube.com/watch?v=MQfVCY9qgEU&list=PLrE-FZIEEls1-c7QifZYzeq50Id08FcJo&index=1

참고 자료 : https://gamzachips.tistory.com/79