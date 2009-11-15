#include <stdlib.h>

void bug(char t)
{
	int *array = malloc(1);
	////<exploitable />
	array[1] = t;
}

int main(int argc, char **argv)
{
	bug(**argv);
}
