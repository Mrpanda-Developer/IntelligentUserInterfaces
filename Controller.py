"""
Controller.py: turns BCI or keyboard input into player movement on the maze
grid.

This is a good file to read if you want to understand *when* a move is
accepted, but most usability tweaks (arrow layout, colors, sizes) belong in
UI.py and Config.py instead.
"""
from Config import VEC, WALK_SPEED_PX_S, MOVE_COOLDOWN_S


class Controller:
    """
    Owns the player's position/heading and applies incoming BCI directions
    to it, one grid cell at a time.

    `bci` is any object exposing `poll_direction() -> 'N'|'E'|'S'|'W'|''`.
    Keyboard directions are sent directly by Main.py when keyboard mode is
    active.
    """

    def __init__(self, maze, cell_px=48, walk_speed_px_s=WALK_SPEED_PX_S, bci=None):
        self.maze = maze
        self.cell_px = cell_px
        self.walk_speed = walk_speed_px_s
        self.pos_rc = maze.start
        self.heading = "E"
        self.armed_dir = None
        self.bci = bci
        bci_available = bci is not None and getattr(bci, "inlet", True) is not None
        self.control_mode = "bci" if bci_available else "keyboard"

        # --- move debouncing -------------------------------------------------
        # After a successful step we ignore new directions for
        # `_move_cooldown` seconds. This stops a single sustained BCI
        # detection from being read as many rapid-fire moves.
        self._move_cooldown = MOVE_COOLDOWN_S
        self._cd_left = 0.0

        self.step_count = 0
        self.elapsed_time = 0.0

    def _try_step(self, d):
        """
        Attempt to move one cell in direction `d`.
        Returns True and updates position/heading if that cell is walkable,
        otherwise returns False and leaves the player where it was.
        """
        dr, dc = VEC[d]
        nxt = (self.pos_rc[0] + dr, self.pos_rc[1] + dc)
        if self.maze.is_path(nxt):
            self.pos_rc = nxt
            self.heading = d
            self.step_count += 1
            return True
        return False

    def handle_bci(self, dt):
        """Poll the BCI source (if any) and apply a move if one is ready."""
        if self.control_mode != "bci" or not self.bci:
            return

        d = self.bci.poll_direction()  # 'N', 'E', 'S', 'W', or '' (nothing detected)
        if not d:
            return

        self.handle_direction(d)

    def handle_direction(self, d):
        """Apply one direction from the currently active control source."""
        if self._cd_left > 0 or d not in VEC:
            return

        self.armed_dir = d  # remember it so UI.py can highlight the matching arrow
        if self._try_step(d):
            self._cd_left = self._move_cooldown

    def handle_keyboard(self, d):
        """Apply a keyboard direction when keyboard control is active."""
        if self.control_mode == "keyboard":
            self.handle_direction(d)

    def toggle_control_mode(self):
        """Switch between BCI and keyboard input and return the new mode."""
        self.control_mode = "keyboard" if self.control_mode == "bci" else "bci"
        self.armed_dir = None
        return self.control_mode

    def update(self, dt):
        """Advance game state by `dt` seconds. Call once per frame from Main.py."""
        self.elapsed_time += dt
        if self._cd_left > 0:
            self._cd_left = max(0.0, self._cd_left - dt)
        self.handle_bci(dt)