import subprocess
import os
import pathlib

class SolverException(Exception):
    pass

class Solver(object):
    def __init__(self, solver):
        """
        Create a new solver instance.

        Keyword arguments:
        solver -- path to the solver executable to use
        """
        workingdirectory = pathlib.Path(solver).parent
        os.chdir(workingdirectory)

        self.process = subprocess.Popen(
            [solver], bufsize=0, stdin=subprocess.PIPE, stdout=subprocess.PIPE, universal_newlines=True)
        self.write_line("set_end_string END")
        self.wait_line("END")
        self._hand_order = None

    def exit(self):
        self.process.kill()
        self.process.wait(1)
        self.process.__exit__(None, None, None)

    def commands(self, lines):
        for line in lines:
            self.command(line)

    def command(self, line):
        try:
            self.write_line(line)
            return self.read_until_end()
        except SolverException:
            self.read_until_end()
            raise

    def commands(self, lines):
        for line in lines:
            response = self.command(line)
            for res in response:
                print(res)

    def write_line(self, line):
        self.write_lines([line])

    def write_lines(self, lines):
        self.process.stdin.write("\n".join(lines))
        self.process.stdin.write("\n")

    def wait_line(self, target):
        self.read_until(target)

    def read_until_end(self):
        return self.read_until("END")

    def read_until(self, target):
        lines = []
        while True:
            line = self.read_line()
            if line.find("problems with your license") == 0:
                raise SolverException(line)
            if line.find("ERROR") == 0 or line.find("Piosolver directory") > 0:
                raise SolverException(line)
            if line.strip() == target.strip():
                return lines
            else:
                lines.append(line.strip())

    def read_line(self):
        line = self.process.stdout.readline()
        if not line:
            raise Exception(f"Unexpected end of output.")
        return line
