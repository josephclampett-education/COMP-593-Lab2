## License: Apache 2.0. See LICENSE file in root directory.
## Copyright(c) 2015-2017 Intel Corporation. All Rights Reserved.

###############################################
##      Open CV and Numpy integration        ##
###############################################

import pyrealsense2 as rs
import numpy as np
import cv2

# Configure depth and color streams
pipeline = rs.pipeline()
config = rs.config()

# Get device product line for setting a supporting resolution
pipeline_wrapper = rs.pipeline_wrapper(pipeline)
pipeline_profile = config.resolve(pipeline_wrapper)
device = pipeline_profile.get_device()
device_product_line = str(device.get_info(rs.camera_info.product_line))

found_rgb = False
for s in device.sensors:
	if s.get_info(rs.camera_info.name) == 'RGB Camera':
		found_rgb = True
		break
if not found_rgb:
	print("The demo requires Depth camera with Color sensor")
	exit(0)

config.enable_stream(rs.stream.depth, 640, 480, rs.format.z16, 30)
config.enable_stream(rs.stream.color, 640, 480, rs.format.bgr8, 30)

# ArUco
arucoDict = cv2.aruco.getPredefinedDictionary(cv2.aruco.DICT_6X6_250)
arucoParams = cv2.aruco.DetectorParameters()
arucoDetector = cv2.aruco.ArucoDetector(arucoDict, arucoParams)

# Start streaming
pipeline.start(config)

try:
	while True:

		# Wait for a coherent pair of frames: depth and color
		frames = pipeline.wait_for_frames()
		depth_frame = frames.get_depth_frame()
		color_frame = frames.get_color_frame()
		if not depth_frame or not color_frame:
			continue

		# Convert images to numpy arrays
		depth_image = np.asanyarray(depth_frame.get_data())
		color_image = np.asanyarray(color_frame.get_data())

		# ArUco Detection
		corners, ids, rejected = arucoDetector.detectMarkers(color_image)
		color_image = cv2.aruco.drawDetectedMarkers(color_image, corners, ids)

		# ================================
		# DRAWING
		# ================================

		depthIntrinsics = depth_frame.profile.as_video_stream_profile().intrinsics
		
		# for i, cornerSet in enumerate(corners):
		#     print(f"======== ID: {ids[i]} ========\n")

		#     assert(cornerSet.shape[0] == 1)
		#     for j, corner in enumerate(cornerSet[0, ...]):
		#         (cameraX, cameraY) = corner
		#         cameraZ = depth_frame.get_distance(cameraX, cameraY)
		#         pointWS = rs.rs2_deproject_pixel_to_point(depthIntrinsics, [cameraX, cameraY], cameraZ)

		#         print(f"PointCS[{j}]: [{cameraX}, {cameraY}, {cameraZ}]")
		#         print(f"PointWS[{j}]: {pointWS}\n")

		depthData = depth_frame.get_data()
		depthBuffer = np.zeros(depth_image.shape)
		for y in range(depth_image.shape[0]):
			for x in range(depth_image.shape[1]):
				cameraZ = depth_frame.get_distance(x, y)
				depthBuffer[y, x] = cameraZ

				
		# ================================

		# Apply colormap on depth image (image must be converted to 8-bit per pixel first)
		depth_colormap = cv2.applyColorMap(cv2.convertScaleAbs(depthBuffer, alpha=20), cv2.COLORMAP_HOT)

		depth_colormap_dim = depth_colormap.shape
		color_colormap_dim = color_image.shape

		# If depth and color resolutions are different, resize color image to match depth image for display
		if depth_colormap_dim != color_colormap_dim:
			resized_color_image = cv2.resize(color_image, dsize=(depth_colormap_dim[1], depth_colormap_dim[0]), interpolation=cv2.INTER_AREA)
			images = np.hstack((resized_color_image, depth_colormap))
		else:
			images = np.hstack((color_image, depth_colormap))

		# Show images
		cv2.namedWindow('RealSense', cv2.WINDOW_AUTOSIZE)
		cv2.imshow('RealSense', images)
		cv2.waitKey(1)

finally:

	# Stop streaming
	pipeline.stop()