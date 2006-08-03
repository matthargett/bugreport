#include <stdlib.h>

static char t;

int main(int argc, char **argv)
{
	t = **argv;
	void *p = malloc(16);
	////<exploitable />
	((char *)p)[16] = t;
}
