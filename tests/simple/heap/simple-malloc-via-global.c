#include <stdlib.h>

static const size_t size = 260;

int main(int argc, char **argv)
{
	void *p = malloc(size);
	((char *)p)[16] = 0;
}
