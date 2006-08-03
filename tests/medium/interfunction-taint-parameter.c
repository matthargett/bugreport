#include <stdlib.h>

void bug(char t)
{
	int *array = malloc(0);
	////<exploitable />
	array[0] = t;
}

int main(int argc, char **argv)
{
	bug(**argv);
}
