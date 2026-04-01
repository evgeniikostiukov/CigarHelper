from playwright.sync_api import sync_playwright

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    page = browser.new_page(viewport={"width": 1280, "height": 800})

    for port in [3001, 3000, 5173, 5174]:
        try:
            page.goto(f'http://localhost:{port}/', timeout=5000)
            print(f"Connected on port {port}")
            break
        except:
            continue

    page.wait_for_load_state('networkidle', timeout=10000)
    page.wait_for_timeout(2000)
    page.screenshot(path='scripts/header_full.png', clip={"x": 0, "y": 0, "width": 1280, "height": 80})

    info = page.evaluate("""() => {
        const list = document.querySelector('.p-menubar-root-list');
        const menubar = document.querySelector('.p-menubar');
        if (!list) return { error: 'list not found', html: document.querySelector('.app-header')?.innerHTML?.substring(0, 500) };
        const ls = window.getComputedStyle(list);
        const ms = menubar ? window.getComputedStyle(menubar) : {};
        const items = [...list.querySelectorAll('.p-menuitem')].map(el => ({
            text: el.querySelector('.p-menuitem-label')?.textContent,
            right: el.getBoundingClientRect().right,
        }));
        return {
            list_display: ls.display,
            list_flexWrap: ls.flexWrap,
            list_overflowX: ls.overflowX,
            list_width: list.offsetWidth,
            list_scrollWidth: list.scrollWidth,
            menubar_flexWrap: ms.flexWrap,
            items,
        };
    }""")
    print("DOM info:", info)
    browser.close()
