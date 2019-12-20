SET start=%date% %time%
@echo Started: %start% >> 50mil_test.txt
dotnet run master >> 50mil_test.txt
@echo Started: %start% >> 50mil_test.txt
@echo Completed: %date% %time% >> 50mil_test.txt
@pause