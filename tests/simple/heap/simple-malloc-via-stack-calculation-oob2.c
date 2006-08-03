#include <stdlib.h>

int main(int argc, char **argv)
{
	size_t size = 16;
	void *p = malloc(size - 8);
	((char *)p)[16] = 0;
}
