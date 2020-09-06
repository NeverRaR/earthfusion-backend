from flask import Flask, request
import sys
import subprocess, signal
from subprocess import Popen
import os

app = Flask(__name__)

def _kill_earthfusion():
    p = subprocess.Popen(['ps', '-A'], stdout=subprocess.PIPE)
    out, err = p.communicate()
    out_utf_8 = out.decode('utf-8')
    for line in out_utf_8.splitlines():
        if 'earthfusion-bac' in line:
            pid = int(line.split(None, 1)[0])
            os.kill(pid, signal.SIGTERM)
            return "successfully killed"
    return "nothing here"

def _start_earthfusion():
    command_pre = ['/usr/bin/git', 'pull']
    job_pre = Popen(command_pre)
    output_pre = job_pre.communicate()
    print(output_pre)
    command = ['/bin/bash', './rebuild_and_start.sh']
    job = Popen(command)
    print(job)
    if job:
        return "looks good"
    else:
        return "bad"
    return "what"


@app.route('/api/earthfusion_ctl/kill', methods=['POST'])
def kill_earthfusion():
    return _kill_earthfusion()


@app.route('/api/earthfusion_ctl/stop', methods=['POST'])
def stop_earthfusion():
    return _kill_earthfusion()


@app.route('/api/earthfusion_ctl/start', methods=['POST'])
def start_earthfusion():
    return _start_earthfusion()


@app.route('/api/earthfusion_ctl/stop_pull_start', methods=['POST'])
def stop_pull_start_earthfusion():
    line_1 = _kill_earthfusion()
    line_2 = _start_earthfusion()
    return (line_1 + line_2)

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=6000, ssl_context=("fullchain.pem", "privkey.pem"))
