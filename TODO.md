TODO
====

* Merge pins using Bounds not points
* Merge standalone pins to element pins (when mowing over element show element pins and enable merging)
* Merge pins to lines if not merging to line start or end (nearest point)
* Diconnect wire from element pin (create temp standalone pin) use start/end pin bounds and move detection
* When moving disconnected wire (start or end pin not connected) reconnected to elements or wires when bounds intersect
* Add automatic line and element orto option to align all elements in grid and have all lines straight (as old sketchpad)
  http://en.wikipedia.org/wiki/Sketchpad
* Filter touch event stream (when moving) to optimize redrawing
* Use element pool for Parses when creating new elements (also other cases of element creation)
* Wires should be drawn in lower Z order then other elements (below elements)
* Handle touch math in drawing thread (maybe by add some abstracted mode enums)
* Add bigger bounding box for standalone pins
* Add undo/redo widget to canvas (drawn on canvas using visible screen area - placed in top right corner)
* Add insert widget to canvas (drawn on canvas using visible screen area - placed in top right corner)
* Draw insert widget using dark transparent background draw over canvas elements
* Add option to cancel drawing standalone pins
* Add snap option
* Add grid
* Add zoom to fit option and enable it on create surface
* Add solution and project strucure
* Add export solution/project or diagram option (using android share functionality)
* Add bounding box for elements (ie. for gate box should be width/height + pin radius)
* When finished editing check for changes (is diagram dirty flag), ask user to save/cancel/discard changes