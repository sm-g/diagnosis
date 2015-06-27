@echo off
rem app, build, setup, tests. always make client setup (common dlls are form client Output)
call build.cmd client 0 1 0
call build.cmd server 0 1 0