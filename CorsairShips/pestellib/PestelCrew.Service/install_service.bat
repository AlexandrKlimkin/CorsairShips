@echo off
sc create PestelCrew.Service displayName=PestelCrew.Service binpath="%~dp0PestelCrew.Service.exe %~dp0services.list"