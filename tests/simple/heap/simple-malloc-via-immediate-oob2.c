#include <stdlib.h>

int main(int argc, char **argv)
{
	void *p = malloc(16);
	((char *)p)[-1] = 0;
}
