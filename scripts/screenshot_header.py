from playwright.sync_api import sync_playwright
import os

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    page = browser.new_page(viewport={"width": 1280, "height": 800})
    page.goto('http://localhost:3001/')
    page.wait_for_load_state('networkidle')
    page.screenshot(path='scripts/header_1280.png', clip={"x": 0, "y": 0, "width": 1280, "height": 70})

    page.set_viewport_size({"width": 1024, "height": 768})
    page.screenshot(path='scripts/header_1024.png', clip={"x": 0, "y": 0, "width": 1024, "height": 70})

    page.set_viewport_size({"width": 768, "height": 1024})
    page.screenshot(path='scripts/header_768.png', clip={"x": 0, "y": 0, "width": 768, "height": 70})

    browser.close()
    print("Screenshots saved")
