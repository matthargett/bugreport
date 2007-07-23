#include <stdio.h>

template<unsigned N> void f()
{
  char buf[N];
  buf[N+1] = 0;
  printf(buf);
};

int main()
{
  f<16>();
}
