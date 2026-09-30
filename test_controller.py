import unittest

from Controller import Controller


class DummyMaze:
    start = (2, 2)

    def is_path(self, pos):
        return True


class DummyBCI:
    def __init__(self, directions):
        self._directions = list(directions)
        self.inlet = object()

    def poll_direction(self):
        if not self._directions:
            return ""
        return self._directions.pop(0)


class ControllerPauseTests(unittest.TestCase):
    def test_bci_pause_blocks_direction(self):
        maze = DummyMaze()
        ctrl = Controller(maze, bci=DummyBCI(["N"]))
        ctrl.paused_bci = True

        ctrl.handle_bci(0.016)

        self.assertEqual(ctrl.pos_rc, maze.start)

    def test_keyboard_still_works_in_keyboard_mode(self):
        maze = DummyMaze()
        ctrl = Controller(maze, bci=DummyBCI(["N"]))
        ctrl.toggle_control_mode()

        ctrl.handle_keyboard("N")

        self.assertNotEqual(ctrl.pos_rc, maze.start)


if __name__ == "__main__":
    unittest.main()
