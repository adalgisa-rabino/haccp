from cvzone.HandTrackingModule import HandDetector
import cv2
import socket
import numpy as np

cap = cv2.VideoCapture(0)
cap.set(3, 1280)
cap.set(4, 720)

# Display settings (logical canvas size when window size cannot be queried)
DISPLAY_W, DISPLAY_H = 1280, 720
cv2.namedWindow("Image", cv2.WINDOW_NORMAL)
cv2.resizeWindow("Image", DISPLAY_W, DISPLAY_H)

# Try to set aspect ratio (may be ignored by some backends)
try:
    cv2.setWindowProperty("Image", cv2.WND_PROP_ASPECT_RATIO, cv2.WINDOW_KEEPRATIO)
except Exception:
    pass

success, img = cap.read()
h, w, _ = img.shape
detector = HandDetector(detectionCon=0.8, maxHands=2)

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5052)

# Debug actual camera capture size
print("Requested capture:", 1280, 720)
print("Actual capture:", int(cap.get(3)), int(cap.get(4)))

while True:
    success, img = cap.read()
    if not success or img is None:
        continue

    # Hand detection
    hands, img = detector.findHands(img)
    data = []
    if hands:
        hand = hands[0]
        lmList = hand["lmList"]
        for lm in lmList:
            data.extend([lm[0], h - lm[1], lm[2]])
        sock.sendto(str.encode(str(data)), serverAddressPort)

    # print(data)
    # Get camera size
    cam_h, cam_w = img.shape[:2]

    # Query the actual window size; fallback to DISPLAY_W/ H if not available
    try:
        _, _, win_w, win_h = cv2.getWindowImageRect("Image")
        if win_w <= 0 or win_h <= 0:
            win_w, win_h = DISPLAY_W, DISPLAY_H
    except Exception:
        win_w, win_h = DISPLAY_W, DISPLAY_H

    # Preserve aspect ratio and letterbox into the current window size
    scale = min(win_w / cam_w, win_h / cam_h)
    new_w = int(cam_w * scale)
    new_h = int(cam_h * scale)
    resized = cv2.resize(img, (new_w, new_h), interpolation=cv2.INTER_AREA)

    canvas = np.zeros((win_h, win_w, 3), dtype=resized.dtype)
    x_off = (win_w - new_w) // 2
    y_off = (win_h - new_h) // 2
    canvas[y_off:y_off + new_h, x_off:x_off + new_w] = resized

    cv2.imshow("Image", canvas)
    if cv2.waitKey(1) & 0xFF == 27:  # ESC to quit
        break

cap.release()
cv2.destroyAllWindows()