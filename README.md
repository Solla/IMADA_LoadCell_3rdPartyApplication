# Description
Load cells are used in several types of measuring instruments. 

IMADA has published several models of load cells, such as ZTS-20N.

ZTS-20N has a 2000Hz sampling rate, so it is feasible to measure time-critical data such as response time.

However, the official software does not provide a protocol for time-synchronization.

This program can record the data from IMADA load cells without official software (except its drivers).

So, when we try to measure an Arduino-based actuator's response time, the program will send a request to Arduino and start recording force data simultaneously.

After the actuator actuates, we can get the response time from the force curve since the start-time is when we send a request to Arduino.

The response time we get will cover: 

1) transmission time between Arduino and PC 

2) actuating time of actuators

3) the latency of load cells measuring

4) transmission time between load cells and PC

## Usage Scenarios

(Academic Papers) Reporting devices' response time

(Product Design) Optimizing the force curves of products

(Game Designer) Designing the vibration patterns when game events occur


# How to use

0. Download the code, modify the packet to Arduino in this code.

1. Find the Arduino COM port from **Windows Device Manager**

2. Build the program and run it.

3. Enter the Arduino's COM port (e.g., **COM4**)

4. Enter the Arduino's Baud Rate (e.g., **115200**)

The program will store the results into **.tsv** . 

Open it by Excel/Numbers/other spreadsheet software.

The program will also calculate response time automatically. The results will be printed on screen.

# Extra Material

[Commands and Output Formatting of IMADA Load Cells (Starts from P.15)](https://twilight.mx/manuales/IM-ZTS110-57-Dinam%C3%B3metro%20Digital%20Serie%20ZTS.pdf)