#undef : define된 macro를 undefine해줌

#ifdef,#endif 같이사용 : #ifdef 다음에 있는 parameter가 정의가 되어있다면 #ifdef #endif 사이의 부분을 코드에 포함.<br>
#ifdef INF<br>
int a = INF;<br>
#endif

#ifndef(if not define) : #ifdef와 정확히 반대<br>
#ifndef INF<br>
#define INF 2e9<br>
#endif