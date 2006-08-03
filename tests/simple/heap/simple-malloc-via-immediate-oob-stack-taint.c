#include <stdlib.h>

int main(int argc, char **argv)
{
	char t = **argv;
	void *p = malloc(16);
	////<exploitable />
	((char *)p)[16] = t;
}
