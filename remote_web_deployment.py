from flask import Flask, request, Response
import random
import subprocess
import signal
from subprocess import Popen
import os
from gevent.pywsgi import WSGIServer
import json

idiot_status_code = [
    # Gone
    400,
    # Teapot
    418,
    # Enhance Your Calm
    420,
    # Unavailable For Legal Reasons
    451,
    # Not Implemented
    501
]

earthfusion_pid = 114514

app = Flask(__name__)


def _kill_earthfusion():
    global earthfusion_pid
    print("Killing application...")
    p = subprocess.Popen(['/bin/ps', '-A'], stdout=subprocess.PIPE)
    out, err = p.communicate()
    out_utf_8 = out.decode('utf-8')
    if earthfusion_pid != 114514:
        # pkill -P
        kill_command = ['/usr/bin/pkill', '-P', str(earthfusion_pid)]
        job_kill = Popen(kill_command)
        earthfusion_pid = 114514
        return "successfully killed"
    return "nothing here"


def _start_earthfusion():
    global earthfusion_pid
    print("Pulling git repo......")
    command_pre = ['/usr/bin/git', 'pull']
    job_pre = Popen(command_pre)
    output_pre = job_pre.communicate()
    print(output_pre)
    command = ['/bin/bash', './rebuild_and_start.sh']
    job = Popen(command)
    earthfusion_pid = job.pid
    print(job)
    if job:
        return "looks good"
    return "bad"


def _stop_pull_start_earthfusion():
    line_1 = _kill_earthfusion()
    line_2 = _start_earthfusion()
    return (line_1 + '\n' + line_2)


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
    return _stop_pull_start_earthfusion()


@app.route('/', methods=['GET', 'HEAD', 'POST', 'PUT', 'DELETE', 'CONNECT', 'OPTIONS', 'TRACE', 'PATCH'])
def default():
    return Response("{'you': 'idiot'}", status=random.choice(idiot_status_code), mimetype='application/json')


if __name__ == '__main__':
    port_num = 6000
    try:
        with open('remote_web_deployment_config.json') as json_file:
            json_data = json.load(json_file)
        port_num = json_data['earthfusion']['remote_web_deployment']['port_num']
    except FileNotFoundError:
        print("Config file not found. Fallback to default.")
    print("Listening on port " + str(port_num))
    _start_earthfusion()
    https_server = WSGIServer(
        ('', port_num), app, keyfile='privkey.pem', certfile='fullchain.pem')
    https_server.serve_forever()
    # app.run(host='0.0.0.0', port=6000, ssl_context=("fullchain.pem", "privkey.pem"))
