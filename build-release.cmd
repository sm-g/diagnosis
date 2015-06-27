@echo off
rem app, build, setup, tests
call build.cmd client 1 0 0
call build.cmd server 1 0 0