@echo off
rem app, build, setup. always make client setup (common dlls are form client Output)
call build.cmd client 1 1
call build.cmd server 1 1