#include <stdlib.h>

int main(int argc, char **argv)
{
	char t = **argv;
	void *p = malloc(16);
	((char *)p)[15] = t;
}
