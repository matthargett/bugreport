#include <stdlib.h>

int main(int argc, char **argv)
{
	size_t *size = malloc(sizeof(size_t));
	*size = 16;
	void *p = malloc(*size);
	((char *)p)[16] = 0;
}
