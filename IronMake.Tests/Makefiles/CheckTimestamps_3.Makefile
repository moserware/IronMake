# By the time we get to this makefile, nothing should need to be done

all: C 

C: B
	@echo making C
	@echo C contents > C

B: A
	@echo making B
	@echo B contents > B

A:
	@echo making A
	@echo A contents > A
