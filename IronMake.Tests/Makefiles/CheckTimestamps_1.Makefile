# Make sure that file timestamps are checked

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
