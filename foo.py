from math import *

AgingTime = 70 * 426.09 * 21600
MaturationTime = 14 * 426.09 * 21600
CyclePeriod = 56 * 21600
GestationPeriod = 265 * 21600
OvulationTime = 0.5 * CyclePeriod
EggLife = 3 * 21600
SpermLife = 3 * 3600

def countbits (x):
    count = 0
    while x > 0:
        count += x & 1
        x >>= 1
    return count

def factor (ind, time, a, b):
    c = a & b
    n = ((c & 0x10) >> 3) - 1
    k = countbits (c & (0xf >> (3 - ind)))
    o = k > 0 and 1 or 0
    x0 = log (EggLife)
    x1 = log (AgingTime)
    x = abs(log (time) - x0)
    m = 0.4 / (x1 - x0)
    y = o * 0.5*n * m * x ** (1 + 0.2 * k)
    return exp (y)

a = 16
#a = 0
for i in range(2):
    print factor (0, SpermLife * 1.2, i+a, i+16) * SpermLife / (3600)
print

for i in range(2):
    print factor (0, EggLife * 1.2, i+a, i+16) * EggLife / (21600)
print

for i in range(2):
    print factor (0, OvulationTime, i+a, i+16) * OvulationTime / (21600)
print

for i in range(2):
    print factor (0, CyclePeriod, i+a, i+16) * CyclePeriod / (21600)
print

for i in range(4):
    print factor (1, GestationPeriod, i+a, i+16) * GestationPeriod / (6*21600)
print
for i in range(8):
    print factor (2, MaturationTime, i+a, i+16) * MaturationTime / (426.09*21600)
print
for i in range(16):
    print factor (3, AgingTime, i+a, i+16) * AgingTime / (426.09*21600)
