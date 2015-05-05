@echo off
rem app, build, setup. always make client setup (common dlls are form client Output)
call build.cmd client 0 1
call build.cmd server 0 1