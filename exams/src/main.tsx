import ReactDOM from 'react-dom/client';
import './index.css';
import { App } from './main.app';

const rootElement = document.getElementById('root')!;
const root = ReactDOM.createRoot(rootElement);
root.render(<App />);
