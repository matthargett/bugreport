#include <stdlib.h>

int main(int argc, char **argv)
{
	size_t size = 15;
	
	void *p = malloc(size + argc);

	////<exploitable />
	((char *)p)[size] = **argv;
}
