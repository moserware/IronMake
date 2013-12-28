# Simple check that the topological sorting happens
# and that tasks aren't repeated

all: D 

D: C
	@echo making D

C: B A
	@echo making C

A:
	@echo making A

B: A
	@echo making B
