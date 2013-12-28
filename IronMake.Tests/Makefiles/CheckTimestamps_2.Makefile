# Delete a file and make sure things get recreated

all: delC C

delC:
	@del C
	@SLEEP 1  # ensure we get different timestamps

C: B
	@echo making C
	@echo C contents > C

B: A
	@echo making B
	@echo B contents > B

A:
	@echo making A
	@echo A contents > A
