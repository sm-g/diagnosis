@echo off
rem app, build, setup, tests. always make client setup (common dlls are form client Output)
call build.cmd client 1 1 0
call build.cmd server 1 1 0