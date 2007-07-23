#include <stdio.h>

int main(int argc, char **argv)
{
	char p[16];
	p[-1] = 0;

	printf(p);

	return 0;
}
